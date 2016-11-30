using System;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace CS422
{
	public class WebRequest
	{
		private NetworkStream _netStream; 
		private Stream _catStream;
		private ValidateRequest _validRequest;

		public string URI{
			get{ return _validRequest.URI;}
		}

		public string HttpVersion{
			get{ return _validRequest.HttpVersion;}
		}

		public Dictionary<string, string> Headers {
			get { return _validRequest.Headers; }
		}

		public string Method{
			get{ return _validRequest.Method;}
		}

		public long BodyLength {
			get {return _catStream.Length; }
		}

		public Stream Body {
			get { return _catStream;}
		}

		public WebRequest (
			NetworkStream netStream, Stream catStream, ValidateRequest validRequest)
		{			
			_netStream = netStream;
			_catStream = catStream;
			_validRequest = validRequest;
		}

		public void WriteNotFoundResponse(string pageHTML)
		{
			string respoString = HttpVersion + " 404 Not Found\r\n"
			                     + "Content-Length: " + pageHTML.Length
			                     + "Content-Type: text/html"
			                     + "\r\n\r\n"
			                     + pageHTML;


			byte[] response = Encoding.ASCII.GetBytes(respoString);

			try
			{
				_netStream.Write (response, 0, response.Length);			
			}
			catch
			{
				return;
			}
		}

		public bool WriteHTMLResponse(string htmlString)
		{
			string respoString = HttpVersion + " 200 OK\r\n"
			                     + "Content-Length: " + htmlString.Length + "\r\n"
			                     + "Content-Type: text/html"
			                     + "\r\n\r\n"
			                     + htmlString;

			byte[] response = Encoding.ASCII.GetBytes(respoString);

			try
			{
				_netStream.Write (response, 0, response.Length);
			}
			catch
			{
				return false;
			}

			return true;
		}

		public bool WritePartialDataResponse(Stream fileData, string contentType)
		{
			string headerval = Headers ["range"];

			string[] rangeHeader = headerval.Split ('=');

			// rangeHeader is [0]='bytes' [1]='first-[second]'
		
			if (rangeHeader.Length != 2)
			{
				//we do not have a valid range request
				WriteNotFoundResponse (string.Empty);
				return false;
			}

			//remove any whitespace at the end of the

			string[] byteRange = rangeHeader [1].Split ('-');
			string firstValue;
			string secondValue = string.Empty;

			if (byteRange.Length == 1)
			{
				// we only have the starting point
				firstValue = byteRange [0];
			}
			else if (byteRange.Length == 2)
			{
				//we have a valid range request with a start and end 
				firstValue = byteRange [0];
				secondValue = byteRange [1];

			}
			else
			{
				// we have an invalid header
				WriteNotFoundResponse (string.Empty);
				return false;
			}

			//Now we have a valid range

			long firstInt = Convert.ToInt64 (firstValue);
			long secondInt = 0;

			if (secondValue != string.Empty)
			{
				secondInt = Convert.ToInt64 (secondValue);
			}

			long amountToRead;

			// Include the second range so addd one
			if (secondInt == 0)
			{
				//read all
				amountToRead = fileData.Length - firstInt;
			}
			else
			{
				amountToRead = secondInt - firstInt + 1;
			}


			if (firstInt >= fileData.Length || secondInt >= fileData.Length
				|| amountToRead < 0)
			{
				WriteNotFoundResponse (string.Empty);
				return false;
			}

			string respoString = HttpVersion + " 206 Partial Content\r\n"
			                     + "Content-Type: " + contentType + "\r\n"
			                     + "Contnet-Length: " + amountToRead + "\r\n"
			                     + "Content-Range: " + firstValue + "-" + secondValue + "/" + fileData.Length
			                     + "\r\n\r\n";

			byte[] response = Encoding.ASCII.GetBytes(respoString);

			//Write our partial data header, if we have a valid range. 
			try
			{
				_netStream.Write (response, 0, response.Length);
			}
			catch
			{
				return false;
			}

			// Go to the location we want to send
			fileData.Seek (firstInt, SeekOrigin.Begin);

			byte[] buf = new byte[1024];
			int readCount;

			while (amountToRead > 0)
			{	
				if (amountToRead < buf.Length)	
				{
					readCount = fileData.Read (buf, 0, (int)amountToRead);
					amountToRead -= readCount;
				}
				else
				{
					readCount = fileData.Read (buf, 0, buf.Length);
					amountToRead -= readCount;
				}
				
				try 
				{
					// The browser may have closed our stream
					_netStream.Write(buf, 0, buf.Length);
				}
				catch
				{
					return false;
				}
			}

			return true;
		}

		public bool WriteGenericFileResponse(Stream fileData, string contentType)
		{
			string respoString = HttpVersion + " 200 OK\r\n"
			                     + "Content-Type: " + contentType + "\r\n"
			                     + "Content-Length: " + fileData.Length
			                     + "\r\n\r\n";

			byte[] response = Encoding.ASCII.GetBytes(respoString);

			try
			{
				_netStream.Write (response, 0, response.Length);
			}
			catch
			{
				return false;
			}

			byte[] buf = new byte[1024];
			int readCount = fileData.Read (buf, 0, buf.Length);

			while (readCount != 0)
			{
				try 
				{
					_netStream.Write(buf, 0, buf.Length);
				}
				catch
				{
					return false;
				}
				readCount = fileData.Read (buf, 0, buf.Length);
			}

			return true;
		}
			
	}
}

