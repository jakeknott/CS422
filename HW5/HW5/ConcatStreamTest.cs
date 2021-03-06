﻿using NUnit.Framework;
using System;
using System.IO;

namespace CS422
{
	[TestFixture ()]
	public class NUnitTestClass
	{
		[Test ()]
		public void TestReadTwoMemStream ()
		{
			Random rnd = new Random();
			Stream defaultStream = new MemoryStream ();
			Stream one = new MemoryStream ();
			Stream two = new MemoryStream ();


			for (int i = 0; i < 920955; i++)
			{
				int number = rnd.Next (1000);
				defaultStream.Write (new byte[]{ (byte)number }, 0, 1);
				one.Write (new byte[]{ (byte)number }, 0, 1);
			}

			for (int i = 0; i < 2000; i++)
			{
				int number = rnd.Next (1000);
				defaultStream.Write (new byte[]{(byte)number }, 0, 1);
				two.Write (new byte[]{ (byte)number }, 0, 1);
			}

			// Now defualtStream is what we expect our concat stream to be
			defaultStream.Seek (0, SeekOrigin.Begin);
			one.Seek (0, SeekOrigin.Begin);
			two.Seek (0, SeekOrigin.Begin);

			ConcatStream conStream = new ConcatStream (one, two);
			int bytesRead = 0;


			for(int i = 0; i <= defaultStream.Length;)
			{
				int randomRead = rnd.Next ((int)defaultStream.Length);
				byte[] readBuf = new byte[randomRead];
				byte[] expectetBuf = new byte[randomRead];

				int amountRead = conStream.Read (readBuf, 0, randomRead);
				int expectedRead = defaultStream.Read (expectetBuf, 0, randomRead);

				Assert.AreEqual (expectetBuf, readBuf);
				Assert.AreEqual (expectedRead, amountRead);

				bytesRead += amountRead;

				i += randomRead;
			}

			// Trying to read past the end of the stream.
			// bytes Read is exacty the length of the stream
			long size = conStream.Length;
			bytesRead += conStream.Read (new byte[1000], 0, 1000);

			Assert.AreEqual (conStream.Length, bytesRead);

			return;
		}

		[Test ()]
		public void TestReadMemAndNoSeek ()
		{
			Random rnd = new Random();
			Stream defaultStream = new MemoryStream ();
			byte[] buf1 = new byte[1000];
			byte[] buf2 = new byte[1500];


			for (int i = 0; i < 1000; i++)
			{
				int number = rnd.Next (1000);
				defaultStream.Write (new byte[]{ (byte)number }, 0, 1);
				buf1 [i] = (byte)number;

			}

			for (int i = 0; i < 1500; i++)
			{
				int number = rnd.Next (1000);
				defaultStream.Write (new byte[]{(byte)number }, 0, 1);
				buf2 [i] = (byte)number;
			}

			// Now defualtStream is what we expect our concat stream to be
			defaultStream.Seek (0, SeekOrigin.Begin);

			Stream one = new MemoryStream (buf1);
			Stream two = new NoSeekMemoryStream (buf2);

			ConcatStream conStream = new ConcatStream (one, two);

			for(int i = 0; i <= defaultStream.Length;)
			{
				int randomRead = rnd.Next ((int)defaultStream.Length);
				byte[] readBuf = new byte[randomRead];
				byte[] expectetBuf = new byte[randomRead];

				int amountRead = conStream.Read (readBuf, 0, randomRead);
				int expectedRead = defaultStream.Read (expectetBuf, 0, randomRead);

				Assert.AreEqual (expectetBuf, readBuf);
				Assert.AreEqual (expectedRead, amountRead);

				i += randomRead;
			}

		}

		[Test]
		public void TestWrite()
		{
			Stream one = new MemoryStream ();
			Stream two = new MemoryStream ();
			Random rnd = new Random();


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

			one = new MemoryStream (new byte[]{5,6,7,8});
			two = new MemoryStream (new byte[]{5,6,7,8});

			conStream = new ConcatStream (one, two);
			conStream.Write (new byte[5]{ 1, 2, 3, 4, 5 }, 0, 5);

			byte[] buf = new byte[5];
			conStream.Seek (0, SeekOrigin.Begin);
			conStream.Read (buf, 0, 5);

			Assert.AreEqual (new byte[5]{ 1, 2, 3, 4, 5 }, buf);

			one = new MemoryStream ();
			two = new MemoryStream ();
			Stream defaultStream = new MemoryStream ();

			for (int i = 0; i < 920955; i++)
			{
				int number = rnd.Next (1000);
				one.Write (new byte[]{ (byte)number }, 0, 1);
			}

			for (int i = 0; i < 2000; i++)
			{
				int number = rnd.Next (1000);
				two.Write (new byte[]{ (byte)number }, 0, 1);
			}

			//byte[] writeBuf = new byte[10000000];
			one.Seek (0, SeekOrigin.Begin);
			two.Seek (0, SeekOrigin.Begin);
			conStream = new ConcatStream (one, two);

			for (int i = 0; i < 10000000 ; i++)
			{
				int num = rnd.Next ();
				defaultStream.Write (new byte[]{ (byte)num }, 0, 1);
				conStream.Write (new byte[]{ (byte)num }, 0, 1);

				//writeBuf[i] = (byte)num;
			}


			//conStream.Write (writeBuf, 0, 10000000);
			//Assert.AreEqual (10000000, conStream.Length);

			conStream.Seek (0, SeekOrigin.Begin);
			defaultStream.Seek (0, SeekOrigin.Begin);


			for(int i = 0; i <= defaultStream.Length;)
			{
				int randomRead = rnd.Next ((int)defaultStream.Length);
				byte[] readBuf = new byte[randomRead];
				byte[] expectetBuf = new byte[randomRead];

				int amountRead = conStream.Read (readBuf, 0, randomRead);
				int expectedRead = defaultStream.Read (expectetBuf, 0, randomRead);

				Assert.AreEqual (expectetBuf, readBuf);
				Assert.AreEqual (expectedRead, amountRead);

				i += randomRead;
			}

			// Failed test that curreupt at posistion 0
			// Seeking then writing to posistion0, seek there and read from position0.
			// I do not know why it is failing that test. This passes. 
			conStream.Seek (0, SeekOrigin.Begin);
			conStream.Write (new byte[] {(byte) 5}, 0, 1);
			conStream.Seek (0, SeekOrigin.Begin);
			byte[] expectedBuf = new byte[1];

			conStream.Read (expectedBuf, 0, 1);

			Assert.AreEqual (new byte[] { (byte)5 }, expectedBuf);
		}
	}
}

