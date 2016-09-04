using System;
using System.IO;

namespace CS422
{
	public class IndexedNumsStream : System.IO.Stream
	{
		private long _length;
		private long _position;

		public IndexedNumsStream (long length)
		{
			SetLength (length);
			_length = Length;
			_position = 0;
		}

		public override bool CanRead {
			get { return true; }
		}

		public override bool CanSeek {
			get { return true; }
		}

		public override bool CanWrite {
			get { return false;}
		}

		public override long Length {
			get { return _length; }
		}

		public override long Position {
			get { return _position; }

			set 
			{
				_position = value;
				if (_position < 0) 
				{
					_position = 0;
				} 
				else if (_position >= _length) 
				{
					_position = _length;
				}
			}
		}

		public override void Flush ()
		{
		}

		public override long Seek (long offset, System.IO.SeekOrigin origin)
		{
			if (origin.Equals (SeekOrigin.End)) 
			{
				Position = _length;
			} 
			else 
			{
				Position = offset;
			}
	
			return Position;
		}

		public override void SetLength (long value)
		{
			if (value < 0) 
			{
				_length = 0;
			}
			else 
			{
				_length = value - 1;
			}
				
		}

		public override int Read (byte[] buffer, int offset, int count)
		{
			int bytesWrote = 0;

			for (int i = 0; i < count; i++) 
			{
				if ((i + offset) < buffer.Length)
				{
					buffer [i + offset] = (byte)(Position % 256);
					Position++;
					bytesWrote++;
				}
			}

			return bytesWrote;
		}

		public override void Write (byte[] buffer, int offset, int count)
		{
		}
	}
}

