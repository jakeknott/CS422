using NUnit.Framework;
using System;
using System.IO;
using System.Text;

namespace CS422
{
	[TestFixture ()]
	public class NUnitTests
	{
		[Test ()]
		public void STDSystemTest()
		{
			if (Directory.Exists ("./NewFolder"))
			{
				Directory.Delete ("./NewFolder", true);
			}

			//Creat root directory
			Directory.CreateDirectory ("./NewFolder");
			StandardFileSystem mySys = StandardFileSystem.Create ("./NewFolder");

			RunTest (mySys);
		}

		[Test]
		public void MemorySystemTest ()
		{
			MemoryFileSystem MemFileSys = new MemoryFileSystem ();

			RunTest (MemFileSys);
		}


		public void RunTest(FileSys422 mySys)
		{
			Dir422 root = mySys.GetRoot ();

			//We should not be able to go above our root.
			Assert.IsNull (root.Parent);

			// Checking that we do not have a file
			Assert.IsFalse (root.ContainsFile ("NewFile.txt", false));
			//create the file
			root.CreateFile ("NewFile.txt");
			// Check that we can find it.
			Assert.IsTrue (root.ContainsFile ("NewFile.txt", false));

			// Same with directory
			Assert.IsFalse (root.ContainsDir ("SubDir", false));
			Dir422 subDir = root.CreateDir ("SubDir");
			Assert.IsTrue (root.ContainsDir ("SubDir", false));

			//Creating a file in a sub dir
			subDir.CreateFile ("subText.txt");

			// Testing the recursive methods on files
			Assert.IsFalse (root.ContainsFile ("subText.txt", false));
			Assert.IsTrue (root.ContainsFile ("subText.txt", true));

			//Testing recurcive method on dirs
			subDir.CreateDir ("newSubDir");

			Assert.IsFalse (root.ContainsDir ("newSubDir", false));
			Assert.IsTrue (root.ContainsDir ("newSubDir", true));

			//Checking getDir
			Dir422 recivedDir = root.GetDir ("InvalidDir");
			Assert.IsNull (recivedDir);
			recivedDir = root.GetDir ("SubDir");
			Assert.AreEqual ("SubDir", recivedDir.Name);

			// Checking that if a file does not exist we return null,
			// otherwise we recived the file we wanted. 
			File422 recidedFile = root.GetFile ("InvalidFile");
			Assert.IsNull (recidedFile);
			recidedFile = root.GetFile ("NewFile.txt");
			Assert.AreEqual ("NewFile.txt", recidedFile.Name);

			//Checking the name validation function.
			// All of these methods use the same Validate Name method.
			Assert.IsNull (subDir.CreateFile ("file/New.txt"));
			Assert.IsNull (subDir.CreateDir ("file/New"));

			string bufString = "hello world";
			byte[] buff = ASCIIEncoding.ASCII.GetBytes (bufString);
			var writeStream = recidedFile.OpenReadWrite ();
			writeStream.Write (buff, 0, 11);

			var readStream = recidedFile.OpenReadOnly ();
			Assert.IsNull (readStream);

			writeStream.Dispose ();

			readStream = recidedFile.OpenReadOnly ();
			Assert.IsNotNull (readStream);

			//First read 'hello ' from each stream
			byte[] readBuf = new byte[6];
			readStream.Read (readBuf, 0, 6);
			Assert.AreEqual ("hello ", ASCIIEncoding.ASCII.GetString (readBuf));

			//Having two streams open for read
			var readStream2 = recidedFile.OpenReadOnly ();
			Assert.IsNotNull (readStream2);

			byte[] readBuf2 = new byte[6];
			readStream2.Read (readBuf2, 0, 6);
			Assert.AreEqual ("hello ", ASCIIEncoding.ASCII.GetString (readBuf2));

			//Next read 'world' from each stream
			readBuf = new byte[5];
			readStream.Read (readBuf, 0, 5);
			Assert.AreEqual ("world", ASCIIEncoding.ASCII.GetString (readBuf));

			readBuf2 = new byte[5];
			readStream2.Read (readBuf2, 0, 5);
			Assert.AreEqual ("world", ASCIIEncoding.ASCII.GetString (readBuf2));

			//try to open a stream to write while there are streams open for read
			writeStream = recidedFile.OpenReadWrite ();
			Assert.IsNull (writeStream);

			//Close streams and try again
			readStream.Close ();
			readStream2.Close ();

			writeStream = recidedFile.OpenReadWrite ();
			Assert.IsNotNull (writeStream);
		}

	}
}

