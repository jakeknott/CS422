using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;

namespace CS422
{
	public class WebServer
	{
		private static string _uri;

		public WebServer ()
		{
		}

		/// <summary>
		/// Start the specified port using the responseTemplate to write to it.
		/// </summary>
		/// <param name="port">Port.</param>
		/// <param name="responseTemplate">Response template.</param>
		public static bool Start (int port, string responseTemplate)
		{
			TcpListener listener = new TcpListener(IPAddress.Any, port);
			listener.Start();
			TcpClient client = listener.AcceptTcpClient();

			if (!client.Connected)
			{
				return false;
			}

			NetworkStream stream = client.GetStream();

			bool isValidHTTP = false;

			if (IsVaid(stream))
			{
				isValidHTTP = true;
				string formatedString = string.Format (responseTemplate, "11398813", DateTime.Now, _uri);
				stream.Write (Encoding.ASCII.GetBytes (formatedString), 0, formatedString.Length);
			}

			stream.Dispose ();
			client.Close ();
			listener.Stop ();

			return isValidHTTP;
		}
			
		/// <summary>
		/// Determines if the HTTP in the readbuff is valid.
		/// </summary>
		/// <returns><c>true</c> if is vaid HTTP the specified readBuff; otherwise, <c>false</c>.</returns>
		/// <param name="readBuff">Read buff.</param>
		public static bool IsVaid(Stream stream)
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
		public static bool IsValidRequestLine(Stream stream)
		{
			if (IsValidMethod (stream)
				&& IsValidURI (stream)
				&& IsValidHTTPVersion (stream))
			{
				return true;
			}

			return false;
		}

		private static bool IsValidHTTPVersion(Stream stream)
		{
			byte[] buf = new byte[1];
			int bytesRead = stream.Read (buf, 0, 1);

			// Read the first byte. 
			string firstChar = ASCIIEncoding.ASCII.GetString (buf);

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
			while (bytesRead != 0)
			{
				// Add character to the string read so far. 
				readSoFar += ASCIIEncoding.ASCII.GetString (buf);

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
					return true;
				}

				// read the next byte. 
				bytesRead = stream.Read (buf, 0, 1);
			}

			// if we have are here then we have recived a read of 0 bytes but have not read our expected string.
			return false;
		}

		private static bool IsValidURI(Stream stream)
		{
			byte[] buf = new byte[1];
			stream.Read (buf, 0, 1);

			// Get the first byte. 
			string read = ASCIIEncoding.ASCII.GetString (buf);
			string requestedURI = read;

			// If we get a space, then that means there are two spaces, one in the method and one here,
			// return false. -+
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
					stream.Read (buf, 0, 1);
					read = ASCIIEncoding.ASCII.GetString (buf);

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
	
		private static bool IsValidMethod(Stream stream)
		{
			string expectedStart = "GET ";
			string readSoFar = string.Empty;
			byte[] buf = new byte[1];
			int bytesRead = 0;

			// Read the first byte. 
			bytesRead = stream.Read (buf, 0, 1);

			// While we are still have stuff to read and we have not already 
			// found our expected string.
			while (bytesRead != 0)
			{
				// Add character to the string read so far. 
				readSoFar += ASCIIEncoding.ASCII.GetString (buf);

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
					return true;
				}

				// read the next byte. 
				bytesRead = stream.Read (buf, 0, 1);
			}

			// If we are here, then we have not found our expected string and we cannot read anymore.
			return false;
		}

		/// <summary>
		/// Determines if the header the is valid.
		/// </summary>
		/// <returns><c>true</c> if is valid header the specified headerLine; otherwise, <c>false</c>.</returns>
		/// <param name="headerLine">Header line.</param>
		public static bool IsValidHeader (Stream stream)
		{
			byte[] buf = new byte[1];
			string readChar = string.Empty;
	

			stream.Read (buf, 0, 1);
			readChar = ASCIIEncoding.ASCII.GetString (buf);

			// must start with a letter
			if (readChar[0] < 'A' || readChar[0] > 'z')
			{
				return false;
			}

			// Read the first field name
			while(!readChar.Equals (":"))
			{
				// We should not have spaces or tabs in our field name.
				if (readChar.Equals (" ") || readChar.Equals ("\t"))
				{
					return false;
				}

				// otherwise read another character
				stream.Read (buf, 0, 1);
				readChar = ASCIIEncoding.ASCII.GetString (buf);
			}


			// Read all the feild vaule pairs in until we get a \r\n twice
 			while (true)
			{
				stream.Read (buf, 0, 1);
				readChar = ASCIIEncoding.ASCII.GetString (buf);

				if (readChar.Equals ("\r"))
				{
					stream.Read (buf, 0, 1);
					readChar = ASCIIEncoding.ASCII.GetString (buf);

					if (readChar.Equals ("\n"))
					{
						// we have one new line, we need two to exit. 
						stream.Read (buf, 0, 1);
						readChar = ASCIIEncoding.ASCII.GetString (buf);

						if (readChar.Equals ("\r"))
						{
							stream.Read (buf, 0, 1);
							readChar = ASCIIEncoding.ASCII.GetString (buf);

							if (readChar.Equals ("\n"))
							{
								return true;
							}
						}
					}
				}
			}
		}
	}
}

