using System;
using System.IO;

namespace CS422
{
	/// <summary>
	/// Represents a memory stream that does not support seeking, but otherwise has
	/// functionality identical to the MemoryStream class.
	/// </summary>
	public class NoSeekMemoryStream : MemoryStream
	{
		private MemoryStream _memStream;

		public NoSeekMemoryStream(byte[] buffer)
		{
			_memStream = new MemoryStream (buffer);
		}

		public NoSeekMemoryStream(byte[] buffer, int offset, int count) 
		{
			_memStream = new MemoryStream (buffer, offset, count);
		}

		public override long Seek (long offset, SeekOrigin loc)
		{
			throw new NotSupportedException ("Seeking is not supported in the NoSeekMemoryStream class");
		}

		public override long Position {
			get {
				return _memStream.Position;
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public override bool CanSeek {
			get {
				return false;
			}
		}

		public override bool CanWrite {
			get {
				return _memStream.CanWrite;
			}
		}

		public override bool CanRead {
			get {
			  	return _memStream.CanRead;
			}
		}

		public override long Length {
			get {
				return _memStream.Length;
			}
		}

		public override int Read (byte[] buffer, int offset, int count)
		{
			return _memStream.Read (buffer, offset, count);
			//Position = _memStream.Position;
		}

		public override void Write (byte[] buffer, int offset, int count)
		{
			_memStream.Write (buffer, offset, count);
			//Position = _memStream.Position;
		}
	}
}

