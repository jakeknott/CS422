using System;
using System.IO;

namespace CS422
{
	public class ConcatStream : Stream 
	{
		private Stream s1;
		private Stream s2;
		private long _length;
		private bool _usedSecConstruct;
		private long _position;
		private bool _s2HasLength;

		public ConcatStream(Stream first, Stream second)
		{
			_usedSecConstruct = false;
			s1 = first;

			// Start the concat stream at position 0.
			s1.Seek (0, SeekOrigin.Begin);

			// This will throw an exception if s1 does not support length, and it must support it. 
			long temp = s1.Length;

			s2 = second;

			try
			{
				// Test if s2 has a length method. 
				temp = s2.Length;

				// If we are here then we can calculate the length of our concate stream.
				_length = s1.Length + s2.Length;
				_s2HasLength = true;
			}
			catch
			{
				_length = -1;
			}

			//TODO: Check this is correct logic
			_position = 0;
		}

		public ConcatStream(Stream first, Stream second, long fixedLength)
		{
			_usedSecConstruct = true;
			s1 = first;

			// Start the concat stream at position 0.
			s1.Seek (0, SeekOrigin.Begin);

			// This will throw an exception if s1 does not support length, and it must support it. 
			long temp = s1.Length;

			s2 = second;
			_length = fixedLength;
			_position = 0;
		}

		public override bool CanRead {
			get {
				if (s1.CanRead && s2.CanRead)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		public override bool CanSeek {
			get {
				if (s1.CanSeek && s2.CanSeek)
				{
					return true;
				}
				else
				{
					return false;
				}			
			}
		}

		public override bool CanWrite {
			get {
				if (s1.CanWrite && s2.CanWrite)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		public override long Length {
			get {
				if (_length == -1)
				{
					throw new NotImplementedException ();
				}
				else
				{
					if (_usedSecConstruct)
					{
						//If we used the second constructor we just return what they told us 
						// was the length
						return _length;
					}

					//If we are here, then we used the first constructor and we know that the 
					// streams both support the length property

					long l1 = s1.Length;
					long l2 = s2.Length;

					// If we are here then we can dynamically get the length
					return l1 + l2;
				}
			}
		}

		//TODO: check this 
		public override long Position {
			get {
				return _position;
			}
			set {
				if (value < 0)
				{
					throw new ArgumentException ("Cannot set position to negative value");
				}

				_position = value;

				// After position is set, we need to adjust our streams to be in the right section.
				if (_position < s1.Length)
				{
					// we are in our first stream
					s1.Seek (_position, SeekOrigin.Begin);

					// If we move into stream one, then make sure to set the stream two to start reading from the beging.
					//s2.Seek (0, SeekOrigin.Begin);
					s2.Position = 0;
				}
				else
				{
					// We are in the second stream.
					// Posistion - s1.length is the relative position into stream 2. 
					s2.Position = _position - s1.Length; //, SeekOrigin.Begin);

					// If we are in s2, then move s1 to the end of itself.
					s1.Seek (0, SeekOrigin.End);
				}

			}
		}

		public override void Flush ()
		{
			s1.Flush ();
			s2.Flush ();
		}

		public override void SetLength (long value)
		{
			_length = value;
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			if (CanSeek)
			{
				if (origin.Equals (SeekOrigin.Begin))
				{
					Position = offset;
				}
				else if (origin.Equals (SeekOrigin.Current))
				{
					Position += offset;
				}
				else
				{
					Position = Length - offset;
				}

				return Position;
			}
			else
			{
				throw new NotImplementedException ();
			}
		}

		public override int Read (byte[] buffer, int offset, int count)
		{
			int bytesRead = 0;

			// read from the first stream as many as we can.
			bytesRead = s1.Read (buffer, offset, count);
			Position += bytesRead;

			// Read from the second stream
			// This will potentally not read, if the first read got everything.
			// Offset + bytesRead is were in the buf we left off
			// count - bytesRead is the amount of bytes left to read. 
			int bytes = s2.Read (buffer, offset + bytesRead, count - bytesRead);
			Position += bytes;
			bytesRead += bytes;

			return bytesRead;
		}

		public override void Write (byte[] buffer, int offset, int count)
		{
			if (CanWrite)
			{
				if (Position < s1.Length)
				{
					//we are in stream one, then write all we can to stream one
					long spaceLeft = s1.Length - Position;
					if (spaceLeft > count)
					{
						// There is enough room to write it all to stream 1
						s1.Write (buffer, offset, count);
					}
					else
					{
						// write only the space left to stream 1.
						s1.Write (buffer, offset, (int)spaceLeft);

						// update the count we need to write.
						int amountLeft = count - (int)spaceLeft;
				
						// Wrtite the rest to stream 2. 
						// We should be at the begining of stream2, if we are not then we cannot write correctly.
						if (s2.Position != 0)
						{
							// Seek to the correct position, if we cannot seek this will throw an execption.
							s2.Seek (0, SeekOrigin.Begin);
						}

						//No we are at the current position in s2
						if (!_usedSecConstruct || amountLeft < Length - spaceLeft)
						{
							//only write if we used the first constructor, or there is enough space to write to 
							// second stream without going over the size limit
							s2.Write (buffer, offset + (int)spaceLeft, amountLeft);
						}
						else
						{
							// here, we had things to write, but were unable to finish the write to 
							// s2 since we would have to resize the second stream and we were given a fixed length
							throw new ArgumentException ();
						}
					}
				} 
				else
				{
					// Wrtite the all to stream 2. 
					long expectedS2Position = Position - s1.Length;
					if (s2.Position != expectedS2Position)
					{
						// Seek to the correct position, if we cannot seek this will throw an execption.
						s2.Seek (expectedS2Position, SeekOrigin.Begin);	
					}

					//No we are at the current position in s2
					if (!_usedSecConstruct || count < Length - Position)
					{
						//only write if we used the first constructor, or there is enough space to write to 
						// second stream without going over the size limit
						s2.Write (buffer, offset + offset, count);
					}
					else
					{
						// here, we had things to write, but were unable to finish the write to 
						// s2 since we would have to resize the second stream and we were given a fixed length
						throw new ArgumentException ();
					}

				}

				// Increment our position to the aomount we wrote,
				// we will only be here if we suceffuly wrote the bytes to the correct stream.
				Position += count;

			}
			else
			{
				throw new NotImplementedException ();
			}
		}
	}
}

