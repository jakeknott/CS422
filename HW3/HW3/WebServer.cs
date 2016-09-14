using System;
using System.Net;
using System.Net.Sockets;

//remove this
using System.Text;

namespace CS422
{
	public class WebServer
	{
		public WebServer ()
		{
		}

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

			if (IsVaidHTTP(stream))
			{
				isValidHTTP = true;
			}
			else
			{
				
			}

			stream.Dispose ();
			client.Close ();
			listener.Stop ();

			return isValidHTTP;
		}

		/// <summary>
		/// Reads a line from the stream.
		/// </summary>
		/// <returns>The line.</returns>
		/// <param name="stream">Stream.</param>
		public static string ReadLine(NetworkStream stream)
		{
			int bytes;
			string line = string.Empty;
			byte[] readBuff = new byte[1];

			do
			{
				bytes = stream.Read (readBuff, 0, 1);
				if (System.Text.Encoding.UTF8.GetString (readBuff) == "\r")
				{
					stream.Read (readBuff, 0, 1);
					if (System.Text.Encoding.ASCII.GetString (readBuff) == "\n")
					{
						break;
					}
				}

				line += System.Text.Encoding.ASCII.GetString (readBuff);
			} while (bytes > 0);

			return line;
		}

		/// <summary>
		/// Determines if the HTTP in the readbuff is valid.
		/// </summary>
		/// <returns><c>true</c> if is vaid HTTP the specified readBuff; otherwise, <c>false</c>.</returns>
		/// <param name="readBuff">Read buff.</param>
		private static bool IsVaidHTTP(NetworkStream stream)
		{
			bool isValid = false;

			string requestLine = ReadLine (stream);
			string[] requestArray = requestLine.Split (' ');

			if (IsValidRequestLine (requestArray))
			{
				string readLine = string.Empty;
				do
				{
					readLine = ReadLine (stream);
					if (readLine != "")
					{
						if (IsValidHeader (ReadLine (stream)))
						{
							isValid = true;
						}
						else 
						{
							isValid = false;
							break;
						}
					}
				} while (readLine != "");
				
			}

			
			return isValid;
		}

		/// <summary>
		/// Determines if the request line is valid.
		/// </summary>
		/// <returns><c>true</c> if is valid request line the specified requestLine; otherwise, <c>false</c>.</returns>
		/// <param name="requestLine">Request line.</param>
		private static bool IsValidRequestLine(string[] requestLine)
		{
			bool isValid = false;

			// There should be and only be three categories in the request line. 
			if (requestLine.Length == 3)
			{
				if (requestLine [0].Equals ("GET"))
				{
					// After the GET method request line needs a '/'
					if (requestLine [1].StartsWith ("/"))
					{
						// If it does have it, then the last section needs to be an HTTP version
						if (requestLine [2].Equals ("HTTP/1.1"))
						{
							isValid = true;
						}
					}
				}
			}
				

			return isValid;
		}

		/// <summary>
		/// Determines if the header the is valid.
		/// </summary>
		/// <returns><c>true</c> if is valid header the specified headerLine; otherwise, <c>false</c>.</returns>
		/// <param name="headerLine">Header line.</param>
		private static bool IsValidHeader (string headerLine)
		{
			bool isValid = false;
			
			string[] splitString = headerLine.Split (':');

			if (splitString.Length == 2)
			{
				// The end of the header must not be a space. 
				if (!splitString [0].EndsWith (" "))
				{
					string headerValue = splitString [1];

					// Removing all the optional space at the begining of the value string.
					while (headerValue.StartsWith (" ") || headerValue.StartsWith ("\t"))
					{
						headerValue = headerValue.Substring (1);
					}

					// Removing all the optional space at the end of the 
					while (headerValue.EndsWith (" ") || headerValue.EndsWith ("\t"))
					{
						headerValue = headerValue.Substring (0, headerValue.Length - 1);
					}

					isValid = true;
				}
			}

			return isValid;
		}
	}
}

