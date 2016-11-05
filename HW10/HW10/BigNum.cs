using System;
using System.Numerics;
using System.Collections;
using System.Text;

namespace CS422
{
	public class BigNum
	{
		private bool m_isUndefinded;
		private bool m_isNeg;
		private BigInteger m_num;
		private BigInteger m_exp;

		public BigNum(string number)
		{
			MakeBigNumFromString (number);
		}

		public BigNum(double value, bool useDoubleToString)
		{
			//Check if NaN or + or - infinity
			if (double.IsNaN (value) 
				|| double.IsPositiveInfinity (value) 
				|| double.IsNegativeInfinity (value))
			{
				m_isUndefinded = true;
				return;
			}

			if (useDoubleToString)
			{
				MakeBigNumFromString (value.ToString ());
				return;
			}


			// We need to read the biniary bits

			byte[ ] byteArray = BitConverter.GetBytes( value );
			var bits = new BitArray(byteArray);

			if(BitConverter.IsLittleEndian)
			{
				//reverse array
				int length = bits.Length;
				int mid = (length / 2);

				for (int i = 0; i < mid; i++)
				{
					bool bit = bits[i];
					bits[i] = bits[length - i - 1];
					bits[length - i - 1] = bit;
				}
			}

			m_isNeg = bits [0];

			string bitString = "";
			foreach (var b in bits)
			{
				if (b.Equals (true))
				{
					bitString += "1";
				}
				else
				{
					bitString += "0";
				}
			}

			string exponentString = bitString.Substring (1, 11);
			string sigString = bitString.Substring (12, 52);

			// Now we need to change the bits into doubles into strings
			int exponent = Convert.ToInt32 (exponentString, 2) - 1023;

			BigNum sigNum = new BigNum("0");

			for(int i = 0; i < sigString.Length; i++)
			{
				if (sigString[i] == '1')
				{
					sigNum += Pow (2, -(i + 1)); 
				}
			}


			// at the 1 in the sig 1.somthing
			sigNum += new BigNum("1");


			BigNum myNum = (sigNum * Pow (2, exponent));
			m_exp = myNum.Exp;
			m_isNeg = myNum.m_isNeg;
			m_isUndefinded = myNum.IsUndefined;
			m_num = myNum.Num;
		}

		private BigNum (BigInteger exp, BigInteger num, bool negative)
		{
			m_exp = exp;
			m_num = num;
			m_isNeg = negative;
		}

		private BigNum (BigNum toCopy)
		{
			m_exp = toCopy.Exp;
			m_num = toCopy.Num;
			m_isNeg = toCopy.m_isNeg;
		}

		private static BigNum Pow(int Base, int pow)
		{
			if (pow == 0)
			{
				return new BigNum ("1");
			}

			if (pow < 0)
			{
				//This Math.Pow (Base, -pow).ToString () should be a bit intiger since they are both 
				// positive intigers, so it will be lossless. 
				BigNum multiplier = new BigNum (BigInteger.Pow (Base, -pow).ToString ());
				BigNum one = new BigNum("1");

				return one / multiplier;
			}



			return new BigNum(BigInteger.Pow (Base, pow).ToString ());
		}

		private void MakeBigNumFromString (string number)
		{			
			if (number == null)
			{
				throw new ArgumentException ();
			}

			int numOfDecPoints = 0;
			int? decimalIndtex = null;
			int extraExponent = 0;

			// take out the E if it exists
			if (number.Contains ("E"))
			{
				//if it has an E it has a +
				try
				{
					extraExponent = Convert.ToInt32 ( number.Substring (number.IndexOf ('+')));
					extraExponent = -extraExponent;
				}
				catch
				{
					extraExponent = Convert.ToInt32 ( number.Substring (number.IndexOf ('-')));
				}
				number = number.Substring (0, number.IndexOf ('E'));

				int di = number.IndexOf ('.');
				number = number.Remove (di, 1);


				while(0 >= number.Length + extraExponent)
				{
					number = number.Insert (number.Length, "0");
				}

				number = number.Insert (number.Length, ".");
			}

			// Go through the chars in the string
			// checking for valid chars
			for (int i = 0; i < number.Length; i++)
			{
				// Check to see if we have a number
				if (number[i] < '0' || number[i] > '9')
				{
					// We have found a non number
					// See if it is a valid non number
					if (number[i] == '-')
					{
						//If we have a '-' and it is not the first char
						// throw exception
						if (i == 0)
						{
							// If we are here then we have a '-' and it is 
							// at the begging of our number string
							m_isNeg = true;
						}
						else
						{
							throw new ArgumentException ();
						}
					}
					else if (number[i] == '.')
					{
						// If we have a decimal, see if it is the first one,
						// if it is not the first one, throw and exception,
						// we can only have one decimal point in a number
						numOfDecPoints++;
						if (numOfDecPoints > 1)
						{
							throw new ArgumentException ();
						}
						else
						{
							decimalIndtex =  i;
						}
					}
					else
					{
						// Otherwise we have a non valid non number.
						throw new ArgumentException ();
					}
				}	
			}

			if (m_isNeg)
			{
				// Take off the '-'
				number = number.Substring (1);

				//Since we are removing the first char, we need to move our decimal up on
				decimalIndtex--;
			}

			//remove any leading 0's
			while (number[0] == '0')
			{
				number = number.Remove (0, 1);
				decimalIndtex--;

				if (number.Length == 0)
				{
					// Our number is zero
					m_exp = 0;
					m_isNeg = false;
					m_num = 0;
					m_isUndefinded = false;
					return;
				}
			}

			if (number.StartsWith ("."))
			{
				//take the decimal off the front
				number = number.Substring (1);

				//Find the first non zero number
				int i = 0;

				while (number[i] == '0')
				{
					i++;
				}

				// i is at the first non zero element
				m_exp = i;
				m_num = Convert.ToInt64 (number.Substring (i));
			}
			else
			{
				if (decimalIndtex != null)
				{
					//we need to go the other way
					m_exp = (int)decimalIndtex - number.Length + 1;
					number = number.Substring (0, (int)decimalIndtex) + number.Substring ((int)decimalIndtex + 1);
					m_num = BigInteger.Parse (number);
				}
				else
				{
					// No decimals
					m_exp = 0;
					m_num = BigInteger.Parse (number);
				}
			}
		}

		public BigInteger Num {
			get { return m_num; }
		}

		public BigInteger Exp {
			get { return m_exp; }
		}

		public bool IsUndefined { 
			get { return m_isUndefinded; } 
		}

		public override string ToString()
		{
			//spcial case of 0
			if (m_exp == 0 && m_num == 0)
			{
				return "0";
			}

			if (m_isUndefinded)
			{
				return "undefined";
			}

			StringBuilder sb = new StringBuilder ();

			sb.Append (m_num.ToString ());

		
			if(m_exp < 0)
			{

				sb.Insert (sb.Length + (int)m_exp, ".");
			}
			else if (m_exp > 0)
			{
				BigInteger i = m_exp;
				while(i > 0)
				{
					sb.Insert (0, "0");
					i--;
				}

				sb.Insert (0, ".");
			}
			// If m_exp == 0 do not add a decimal to the string

			if (m_isNeg)
			{
				sb.Insert (0, "-");
			}
				
			// erasing all leading and trailing 0's 
			while (sb[sb.Length -1] == '0' && m_exp != 0)
			{
				sb = sb.Remove (sb.Length - 1, 1);
			}

			while(sb[0] == '0')
			{
				sb = sb.Remove (0, 1);
			}

			if (sb[sb.Length -1] == '.')
			{
				//If we have just a decimal at the end, 
				// ex) 2.00000 will go to 2.
				// so we need to remove it to just 2
				sb = sb.Remove(sb.Length -1, 1);
			}

			return sb.ToString ();
		}

		public static BigNum operator+(BigNum lhs, BigNum rhs)
		{
			if (lhs.m_isUndefinded || rhs.m_isUndefinded)
			{
				BigNum undefinedNum = new BigNum("0");
				undefinedNum.m_isUndefinded = true;
				return undefinedNum;
			}
			
			BigInteger eDif = BigInteger.Abs ( lhs.m_exp - rhs.m_exp);

			BigNum shiftedLhs = new BigNum(lhs);
			BigNum shiftedRhs = new BigNum(rhs);
			BigInteger newNum;

			if (lhs.m_exp < rhs.m_exp)
			{
				shiftedRhs.m_num = BigInteger.Pow (10, (int)eDif) * rhs.Num;
				shiftedRhs.m_exp = rhs.Exp- eDif;
			}
			else if (lhs.m_exp > rhs.m_exp)
			{
				shiftedLhs.m_num = BigInteger.Pow (10,(int)eDif) * lhs.Num;
				shiftedLhs.m_exp = lhs.Exp - eDif;
			}
				
			// they are now equal
			if (shiftedLhs.m_isNeg)
			{
				newNum = -shiftedLhs.Num + shiftedRhs.Num;
			}
			else if (shiftedRhs.m_isNeg)
			{
				newNum = shiftedLhs.Num - shiftedRhs.Num;
			}
			else
			{
				newNum = shiftedLhs.Num + shiftedRhs.Num;
			}

			bool newNeg = false;

			if (newNum < 0)
			{
				newNum = -newNum;
				newNeg = true;
			}

			return new BigNum (shiftedRhs.Exp, newNum, newNeg);
				
		}

		public static BigNum operator-(BigNum lhs, BigNum rhs)
		{
			if (lhs.m_isUndefinded || rhs.m_isUndefinded)
			{
				BigNum undefinedNum = new BigNum("0");
				undefinedNum.m_isUndefinded = true;
				return undefinedNum;
			}

			BigNum negRhs = new BigNum(rhs);
			negRhs.m_isNeg = true;

			// adding the negation of right hand side. 
			return lhs + negRhs;
		}

		public static BigNum operator*(BigNum lhs, BigNum rhs)
		{
			if (lhs.m_isUndefinded || rhs.m_isUndefinded)
			{
				BigNum undefinedNum = new BigNum("0");
				undefinedNum.m_isUndefinded = true;
				return undefinedNum;
			}
			
			BigInteger exponent = lhs.m_exp + rhs.m_exp;
			BigInteger num = lhs.m_num * rhs.m_num;

			bool newNumNeg = false;

			if (lhs.m_isNeg || rhs.m_isNeg)
			{
				//atleast one is negative
				if (lhs.m_isNeg && rhs.m_isNeg)
				{
					//if both negative the our new num is not
					newNumNeg = false;
				}
				else
				{
					// only one is negative, so our new num is negative
					newNumNeg = true;
				}
			}

			return new BigNum (exponent, num, newNumNeg);
		}

		public static BigNum operator/(BigNum lhs, BigNum rhs)
		{
			if (lhs.m_isUndefinded || rhs.m_isUndefinded || rhs.ToString () == "0")
			{
				BigNum undefinedNum = new BigNum("0");
				undefinedNum.m_isUndefinded = true;
				return undefinedNum;
			}

			int numPerDitigs = 60;
			BigInteger exponent = lhs.m_exp - rhs.m_exp - numPerDitigs;
			BigInteger percision = BigInteger.Pow (10, numPerDitigs);
			BigInteger num = (percision * lhs.m_num) / rhs.m_num;

			bool newNeg = false;

			if (lhs.m_isNeg || rhs.m_isNeg)
			{
				//atleast one is negative
				if (lhs.m_isNeg && rhs.m_isNeg)
				{
					//if both negative the our new num is not
					newNeg = false;
				}
				else
				{
					// only one is negative, so our new num is negative
					newNeg = true;
				}
			}

			return new BigNum (exponent, num, newNeg);
		}

		private static bool doGt (BigNum lhs, BigNum rhs)
		{
			if (lhs.m_exp == rhs.m_exp)
			{
				return lhs.m_num > rhs.m_num;
			}

			return lhs.m_exp > rhs.m_exp;
		}

		private static bool doGtEq (BigNum lhs, BigNum rhs)
		{
			if (lhs.m_exp == rhs.m_exp)
			{
				return lhs.m_num >= rhs.m_num;
			}

			return lhs.m_exp >= rhs.m_exp;
		}

		private static bool doLt (BigNum lhs, BigNum rhs)
		{
			if (lhs.m_exp == rhs.m_exp)
			{
				return lhs.m_num < rhs.m_num;
			}

			return lhs.m_exp < rhs.m_exp;
		}

		private static bool doLtEq (BigNum lhs, BigNum rhs)
		{
			if (lhs.m_exp == rhs.m_exp)
			{
				return lhs.m_num <= rhs.m_num;
			}

			return lhs.m_exp <= rhs.m_exp;
		}

		public static bool operator>(BigNum lhs, BigNum rhs)
		{
			if (lhs.m_isUndefinded || rhs.m_isUndefinded)
			{
				return false;
			}
			
			if (lhs.m_isNeg && !rhs.m_isNeg)
			{
				return false;
			}

			if (!lhs.m_isNeg && rhs.m_isNeg)
			{
				return true;
			}

			//If both are negative return the opposite
			if (lhs.m_isNeg && rhs.m_isNeg)
			{
				return doLt (lhs, rhs);
			}
			
			return doGt (lhs, rhs);
		}

		public static bool operator>=(BigNum lhs, BigNum rhs)
		{
			if (lhs.m_isUndefinded || rhs.m_isUndefinded)
			{
				return false;
			}
			
			if (lhs.m_isNeg && !rhs.m_isNeg)
			{
				return false;
			}

			if (!lhs.m_isNeg && rhs.m_isNeg)
			{
				return true;
			}

			//If both are negative return the opposite
			if (lhs.m_isNeg && rhs.m_isNeg)
			{
				return doLtEq (lhs, rhs);
			}

			return doGtEq (lhs, rhs);
		}

		public static bool operator<(BigNum lhs, BigNum rhs)
		{
			if (lhs.m_isUndefinded || rhs.m_isUndefinded)
			{
				return false;
			}
			
			return !(lhs >= rhs);
		}

		public static bool operator<=(BigNum lhs, BigNum rhs)
		{
			if (lhs.m_isUndefinded || rhs.m_isUndefinded)
			{
				return false;
			}
			
			return !(lhs > rhs);
		}

		public static bool IsToStringCorrect(double value)
		{
			string dts = value.ToString ();
			BigNum myNum = new BigNum (value, false);

			return dts == myNum.ToString ();
		}
	}
}