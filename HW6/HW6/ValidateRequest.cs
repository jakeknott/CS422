using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace CS422
{
	public class ValidateRequest
	{
		private byte[] buf = new byte[2048];
		private int index; 
		private int bytesInBuf;
		private string _uri;
		private string _method;
		private string _httpVersion;
		private Dictionary<string, string> _headers;

		public Dictionary<string, string> Headers {
			get {return _headers;}
		}

		public int Index {
			get {return index;}
		}

		public int BytesInBuf {
			get {return bytesInBuf;}
		}

		public string URI {
			get {return _uri;}
		}

		public string Method {
			get {return _method;}
		}

		public string HttpVersion {
			get {return _httpVersion;}
		}

		public byte[] Buf {
			get {return buf;}
		}

		public ValidateRequest ()
		{
			index = 0;
			bytesInBuf = 0;

			index = 0; 
			bytesInBuf = 0;
			_uri = string.Empty;
			_method = string.Empty;
			_httpVersion = string.Empty;
			_headers = new Dictionary<string, string>{};
		}

		/// <summary>
		/// Determines if the HTTP in the readbuff is valid.
		/// </summary>
		/// <returns><c>true</c> if is vaid HTTP the specified readBuff; otherwise, <c>false</c>.</returns>
		/// <param name="readBuff">Read buff.</param>
		public bool IsVaid(Stream stream)
		{
			// If we cannot read, then we return false. 
			if (!stream.CanRead)
			{
				return false;
			}

			if (IsValidRequestLine (stream))
			{
				if (IsValidHeader(stream))
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Determines if the request line is valid.
		/// </summary>
		/// <returns><c>true</c> if is valid request line the specified requestLine; otherwise, <c>false</c>.</returns>
		/// <param name="requestLine">Request line.</param>
		public bool IsValidRequestLine(Stream stream)
		{
			if (IsValidMethod (stream)
				&& IsValidURI (stream)
				&& IsValidHTTPVersion (stream))
			{
				return true;
			}

			return false;
		}

		private bool IsValidHTTPVersion(Stream stream)
		{
			//byte[] buf = new byte[1];
			//int bytesRead = stream.Read (buf, 0, 1);

			// Read if nessessary
			if (index >= bytesInBuf)
			{
				// read into our buff, save 'actual' size
				bytesInBuf = stream.Read (buf, 0, buf.Length);

				// Reset index to 0
				index = 0;
			}


			// Read the first byte. 
			// Don't increment index because we want to re read this char later
			byte[] charBuf = new byte[]{ buf [index] };
			string firstChar = ASCIIEncoding.ASCII.GetString (charBuf);

			// If the first character is a space, then we have an invalid request line, 
			// since we have read the space at the end of the uri already.
			if (firstChar.Equals (" "))
			{
				return false;
			}

			// If the first byte is not a space then continue to check if it matches what we expect. 
			string expectedHTTP = "HTTP/1.1\r\n";
			string readSoFar = string.Empty;

			// While we are still have stuff to read and we have not already 
			// found our expected string.
			while (true)
			{
				// Read if nessessary
				if (index >= bytesInBuf)
				{
					// read into our buff, save 'actual' size
					bytesInBuf = stream.Read (buf, 0, buf.Length);

					// Reset index to 0
					index = 0;
				}

				// Add character to the string read so far. 
				charBuf = new byte[]{ buf [index++] };
				readSoFar += ASCIIEncoding.ASCII.GetString (charBuf);

				// If our read so far is NOT at the begining of our expected 
				// Return false.
				if (!expectedHTTP.StartsWith (readSoFar))
				{
					return false;
				}

				// If we have read our expeted string (including the space)
				// Return true.
				if (expectedHTTP.Equals (readSoFar))
				{
					// We have read our expted string, set is valid to true and break.
					_httpVersion = "HTTP/1.1";
					return true;
				}
			}
		}


		private bool IsValidURI(Stream stream)
		{
			//byte[] buf = new byte[1];

			if (index >= bytesInBuf)
			{
				// read into our buff, save 'actual' size
				bytesInBuf = stream.Read (buf, 0, buf.Length);

				// Reset index to 0
				index = 0;
			}

			// Get the first byte. 
			byte[] charBuf = new byte[]{ buf [index++] };
			string read = ASCIIEncoding.ASCII.GetString (charBuf);
			string requestedURI = read;

			// If we get a space, then that means there are two spaces, one in the method and one here,
			// return false.
			if (read.Equals (" "))
			{
				// We do not want a space at the beginging of the uri
				return false;
			}
			else
			{
				// Otherwise, we will read untill we get a space.
				while (!read.Equals (" "))
				{
					if (index >= bytesInBuf)
					{
						// read into our buff, save 'actual' size
						bytesInBuf = stream.Read (buf, 0, buf.Length);

						// Reset index to 0
						index = 0;
					}

					charBuf = new byte[]{ buf [index++] };
					read = ASCIIEncoding.ASCII.GetString (charBuf);

					if (read.Equals ("\r"))
					{
						// you cannot have a \r in with the uri (This may occure, if the request line does not 
						// give a http version, so the uri goes all the way to the end of the line. 
						return false;
					}

					requestedURI += read;
				}

				// When we are here we have read the whole uri and recived a space after it. 
			}

			_uri = requestedURI;
			return true;
		}

		private bool IsValidMethod(Stream stream)
		{
			string expectedStart = "GET ";
			string readSoFar = string.Empty;

			// Read the set of the request.
			bytesInBuf = stream.Read (buf, 0, buf.Length);
			index = 0;

			// While we are still have stuff to read and we have not already 
			// found our expected string.
			while (true)
			{
				// read the next byte. 
				if (index >= bytesInBuf)
				{
					bytesInBuf = stream.Read (buf, 0, buf.Length);
					index = 0;
				}

				// Add character to the string read so far. 
				byte[] charBuf = new byte[]{ buf [index++] };
				readSoFar +=  ASCIIEncoding.ASCII.GetString (charBuf);

				// If our read so far is NOT at the begining of our expected 
				// Return false.
				if (!expectedStart.StartsWith (readSoFar))
				{
					return false;
				}

				// If we have read our expeted string (including the space)
				// Return true.
				if (expectedStart.Equals (readSoFar))
				{
					// We have read "GET "
					_method = "GET";
					return true;
				}
			}
		}

		/// <summary>
		/// Determines if the header the is valid.
		/// </summary>
		/// <returns><c>true</c> if is valid header the specified headerLine; otherwise, <c>false</c>.</returns>
		/// <param name="headerLine">Header line.</param>
		public bool IsValidHeader (Stream stream)
		{
			//byte[] buf = new byte[1];
			string readChar = string.Empty;
			string currentKey = string.Empty;
			string currentValue = string.Empty;

			// read the next byte. 
			if (index >= bytesInBuf)
			{
				bytesInBuf = stream.Read (buf, 0, buf.Length);
				index = 0;
			}

			byte[] charBuf = new byte[] { buf [index++] };
			readChar = ASCIIEncoding.ASCII.GetString (charBuf);

			// must start with a letter
			if (readChar[0] < 'A' || readChar[0] > 'z')
			{
				return false;
			}
				
			// Read the first field name
			while(!readChar.Equals (":"))
			{
				currentKey += readChar;

				// We should not have spaces or tabs in our field name.
				if (readChar.Equals (" ") || readChar.Equals ("\t"))
				{
					return false;
				}

				// read if needed
				if (index >= bytesInBuf)
				{
					bytesInBuf = stream.Read (buf, 0, buf.Length);
					index = 0;
				}

				// otherwise read another character
				charBuf = new byte[] { buf [index++] };
				readChar = ASCIIEncoding.ASCII.GetString (charBuf);
			}
			// currentKey is the first key now (not including the ":")
			bool hasKey = true;

			// Read all the feild vaule pairs in until we get a \r\n twice
			while (true)
			{
				// read if needed
				if (index >= bytesInBuf)
				{
					bytesInBuf = stream.Read (buf, 0, buf.Length);
					index = 0;
				}

				// otherwise read another character
				charBuf = new byte[] { buf [index++] };
				readChar = ASCIIEncoding.ASCII.GetString (charBuf);

				if (readChar.Equals ("\r"))
				{
					// read if needed
					if (index >= bytesInBuf)
					{
						bytesInBuf = stream.Read (buf, 0, buf.Length);
						index = 0;
					}

					// otherwise read another character
					charBuf = new byte[] { buf [index++] };
					readChar = ASCIIEncoding.ASCII.GetString (charBuf);

					if (readChar.Equals ("\n"))
					{
						// read if needed
						if (index >= bytesInBuf)
						{
							bytesInBuf = stream.Read (buf, 0, buf.Length);
							index = 0;
						}

						// otherwise read another character
						charBuf = new byte[] { buf [index++] };
						readChar = ASCIIEncoding.ASCII.GetString (charBuf);

						if (readChar.Equals ("\r"))
						{
							// read if needed
							if (index >= bytesInBuf)
							{
								bytesInBuf = stream.Read (buf, 0, buf.Length);
								index = 0;
							}

							// otherwise read another character
							charBuf = new byte[] { buf [index++] };
							readChar = ASCIIEncoding.ASCII.GetString (charBuf);

							if (readChar.Equals ("\n"))
							{
								return true;
							}
						}
						else
						{
							// we have a single new line, not the end, just a header field
							// Here we have our current key and current value = value + next key;
							string[] temp = currentValue.Split (':');
							currentValue = string.Empty;

							int startIndex = 0;
							//The header key
							if (!hasKey)
							{
								// we already have the current Key
								currentKey = temp [0];
								startIndex = 1;
							}

							for (int i = startIndex; i < temp.Length; i++)
							{
								// Add back the ':' in the middle of the word
								if (i != startIndex && i != temp.Length)
									currentValue += ":";
								currentValue += temp [i];
							}

							while(currentValue.StartsWith ("\t") && currentValue.StartsWith (" "))
							{
								// while there are still spaces at the front of the header
								currentValue += currentValue.Substring (1);
							}

							while(currentValue.EndsWith ("\t") && currentValue.EndsWith (" "))
							{
								// while there are still spaces at the end of the header
								currentValue += currentValue.Substring (0, currentValue.Length - 1);
							}

							_headers.Add (currentKey.ToLower (), currentValue);
							currentValue = readChar;

							hasKey = false;
						}
					}
				}
				else
				{
					// we have a normal char, not a newline or a ':'
					// add it to our current key.
					currentValue += readChar;
				}

				// Even if we read in one line, the readchar would be changed to the next one already, if not, 
				//we have the same char from above. 

			}
		}
	}
}

