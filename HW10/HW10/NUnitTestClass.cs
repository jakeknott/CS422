using NUnit.Framework;
using System;

namespace CS422
{
	[TestFixture ()]
	public class NUnitTestClass
	{
		[Test ()]
		public void TestCase ()
		{
			BigNum myNum = new BigNum("0000.00000124");
			string mystring = myNum.ToString ();

			Assert.AreEqual (".00000124", mystring);

			myNum = new BigNum (45.3265, true);
			Assert.AreEqual ("45.3265", myNum.ToString ());

			myNum = new BigNum ("-45.25600000");
			mystring = myNum.ToString ();
			Assert.AreEqual ("-45.256", mystring);


			myNum = new BigNum("17825.23569874");
			mystring = myNum.ToString ();
			Assert.AreEqual ("17825.23569874", mystring);


			BigNum newNum = new BigNum("256.2314");
			var b = myNum * newNum;
			mystring = b.ToString ();

			b = myNum / newNum;
			mystring = b.ToString ();

			b = myNum + newNum;
			mystring = b.ToString ();

			b = myNum - newNum;
			mystring = b.ToString ();

			BigNum numOne = new BigNum("5");
			BigNum numTwo = new BigNum("10");

			string numOneString = numOne.ToString ();
			Assert.AreEqual ("5", numOneString);

			String numTwoString = numTwo.ToString ();
			Assert.AreEqual ("10", numTwoString);

			Assert.AreEqual ("15", (numOne + numTwo).ToString ());
			Assert.AreEqual ("50", (numOne * numTwo).ToString ());
			Assert.AreEqual ("2", (numTwo / numOne).ToString ());


			BigNum zero = new BigNum("0000000");

			Assert.IsTrue ((myNum / zero).IsUndefined);

			BigNum a = new BigNum("5.2");
			BigNum c = new BigNum("5.2");

			bool compare = a > c;
			Assert.IsFalse (compare);

			compare = a >= c;
			Assert.IsTrue (compare);

			compare = a < c;
			Assert.IsFalse (compare);

			compare = a <= c;
			Assert.IsTrue (compare);


			c = new BigNum("-5.2");

			compare = a > c;
			Assert.IsTrue (compare);

			compare = a >= c;
			Assert.IsTrue (compare);

			compare = a < c;
			Assert.IsFalse (compare);

			compare = a <= c;
			Assert.IsFalse (compare);


			BigNum fromDouble = new BigNum (45.24587, false);

			bool isSame = BigNum.IsToStringCorrect (45.24587);
			Assert.IsFalse (isSame);

			Assert.IsTrue (BigNum.IsToStringCorrect (4.5));
		}
	}
}

