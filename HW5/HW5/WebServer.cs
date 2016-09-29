using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace CS422
{
	public class WebServer : IThreadPool
	{
		private static string _uri;
		private static byte[] buf = new byte[2048];
		private static string _method;
		private static string _httpVersion;
		private static int index; 
		private static int bytesInBuf;
		private static ThreadPool pool;
		private  TcpClient _client; 
		private Dictionary<string, string> services; 

		public WebServer(TcpClient client)
		{
			_client = client;
		}

		/// <summary>
		/// Start the specified port using the responseTemplate to write to it.
		/// </summary>
		/// <param name="port">Port.</param>
		/// <param name="responseTemplate">Response template.</param>
		public static void Start (int port, ushort numThreads)
		{
			index = 0;
			bytesInBuf = 0;

			pool = new ThreadPool (numThreads);


			System.Threading.Thread listenThread = new System.Threading.Thread (Listen);
			listenThread.Start (port);
		}

		private static void Listen(object obj)
		{
			int port = (int)obj;
			
			TcpClient client;
			TcpListener listener = new TcpListener(IPAddress.Any, port);
			listener.Start();


			while(true)
			{
				// accept a client and start a thread with it. 
				client = listener.AcceptTcpClient();
				pool.Add (client);
			}
		}

		internal void AddService(WebService service)
		{
			
		}

		public void ThreadWork()
		{
			WebRequest request = BuildRequest (_client);
			if (request == null)
			{
				_client.GetStream ().Dispose ();
				_client.Close ();
			}
			else
			{
				// we have a valid http request

			}
		}

		private WebRequest BuildRequest(TcpClient client)
		{
			NetworkStream stream = client.GetStream();

			if (IsVaid(stream))
			{
				Stream memoryStream = new MemoryStream (buf, index, bytesInBuf - index);

				// since the network stream cannot seek, we can just give it the stream and all reads will start from where we left off. 
				ConcatStream catStream = new ConcatStream (memoryStream, stream);
				return new WebRequest (stream, catStream, _httpVersion, _uri, _method);
				
				//isValidHTTP = true;
				//string formatedString = string.Format (responseTemplate, "11398813", DateTime.Now, _uri);
				//stream.Write (Encoding.ASCII.GetBytes (formatedString), 0, formatedString.Length);
			}


			//listener.Stop ();

			return null;
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

		private static bool IsValidURI(Stream stream)
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
	
		private static bool IsValidMethod(Stream stream)
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
		public static bool IsValidHeader (Stream stream)
		{
			//byte[] buf = new byte[1];
			string readChar = string.Empty;
	
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
					}
				}
			}
		}
	}
}

