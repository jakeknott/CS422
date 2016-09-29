using NUnit.Framework;
using System;
using System.IO;

namespace CS422
{
	[TestFixture ()]
	public class NUnitTestClass
	{
		[Test ()]
		public void TestRead ()
		{
			Stream one = new MemoryStream ();
			Stream two = new MemoryStream ();

			one.Write (new byte[]{ 1, 2, 3, 4, 5 }, 0, 5);
			two.Write (new byte[]{ 6, 7, 8, 9, 10 }, 0, 5);

			one.Seek (0, SeekOrigin.Begin);
			two.Seek (0, SeekOrigin.Begin);


			ConcatStream conStream = new ConcatStream (one, two);

			Assert.AreEqual (10, conStream.Length);

			Assert.IsTrue (conStream.CanRead);

			byte[] buf = new byte[10];
			conStream.Read (buf, 0, 10);
			Assert.AreEqual (new byte[10]{ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, buf);

			buf = new byte[10];
			conStream.Read (buf, 0, 10);
			Assert.AreEqual (new byte[10], buf);

		}

		[Test]
		public void TestWrite()
		{
			Stream one = new MemoryStream ();
			Stream two = new MemoryStream ();

			one.Write (new byte[]{ 1, 2, 3, 4, 5 }, 0, 5);
			two.Write (new byte[]{ 6, 7, 8, 9, 10 }, 0, 5);

			one.Seek (0, SeekOrigin.Begin);
			two.Seek (0, SeekOrigin.Begin);

			Stream conStream = new ConcatStream (one, two);
			conStream.Seek (0, SeekOrigin.Begin);
			conStream.Seek (4, SeekOrigin.Begin);
			conStream.Seek (7, SeekOrigin.Begin);
			conStream.Seek (10, SeekOrigin.Begin);
			conStream.Seek (0, SeekOrigin.End);
			conStream.Seek (4, SeekOrigin.End);
			conStream.Seek (8, SeekOrigin.End);

			one = new MemoryStream ();
			two = new MemoryStream ();

			conStream = new ConcatStream (one, two);
			conStream.Write (new byte[5]{ 1, 2, 3, 4, 5 }, 0, 5);

			byte[] buf = new byte[5];
			conStream.Seek (0, SeekOrigin.Begin);
			conStream.Read (buf, 0, 5);

			Assert.AreEqual (new byte[5]{ 1, 2, 3, 4, 5 }, buf);
		}
	}
}

