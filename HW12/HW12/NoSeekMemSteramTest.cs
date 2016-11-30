using NUnit.Framework;
using System;
using System.IO;

namespace CS422
{
	[TestFixture ()]
	public class NoSeekMemSteramTest
	{
		[Test ()]
		public void NoSeekTest ()
		{
			byte[] buf = new byte[50];
			Random rnd = new Random ();

			for (int i = 0; i < 50; i++)
			{
				buf [i] = (byte)rnd.Next ();
			}

			Stream noSeekStream = new NoSeekMemoryStream (buf);

			Assert.AreEqual (false, noSeekStream.CanSeek);

			try
			{
				noSeekStream.Seek (10, SeekOrigin.Current);
				throw new InvalidOperationException ("This should not happen");
			}
			catch (NotSupportedException)
			{
				// should catch the execption
			}

			//Cannot set position
			Assert.Throws<NotImplementedException> (delegate {noSeekStream.Position = 10; });
		}
	}
}

