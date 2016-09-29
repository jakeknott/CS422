using System;
using System.IO;

namespace CS422
{
	public class ConcatStream : Stream 
	{
		private Stream s1;
		private Stream s2;
		private long _length;
		private long _posistion;


		public ConcatStream(Stream first, Stream second)
		{
			s1 = first;

			// This will throw an exception if s1 does not support length, and it must support it. 
			long temp = s1.Length;

			s2 = second;

			try
			{
				// Test if s2 has a length method. 
				temp = s2.Length;

				// If we are here then we can calculate the length of our concate stream.
				_length = s1.Length + s2.Length;
			}
			catch
			{
				_length = -1;
			}

			_posistion = 0;
		}

		public ConcatStream(Stream first, Stream second, long fixedLength)
		{
			s1 = first;

			// This will throw an exception if s1 does not support length, and it must support it. 
			long temp = s1.Length;

			s2 = second;
			_length = fixedLength;
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
					return _length;
				}
			}
		}

		public override long Position {
			get {
				return _posistion;
			}
			set {
				_posistion = value;
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

				// No positin is set, we need to adjust our streams to be in the righ section.
				if (Position < s1.Length)
				{
					// we are in our first stream
					s1.Seek (Position, SeekOrigin.Begin);

					// If we move into stream one, then make sure to set the stream two to start reading from the beging.
					s2.Seek (0, SeekOrigin.Begin);
				}
				else
				{
					// We are in the second stream.
					// Posistion - s1.length is the relative position into stream 2. 
					s2.Seek (Position - s1.Length, SeekOrigin.Begin);

					// If we are in s2, then move s1 to the end of itself.
					s1.Seek (0, SeekOrigin.End);
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
				// we need to write 

				if (Position <= s1.Length)
				{
					//we are in stream one, then write all we want to stream one
					s1.Write (buffer, offset, count);
				} 
				else
				{
					// The position is in stream 2, so write everything to stream 2.
					s1.Write (buffer, offset, count);
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

