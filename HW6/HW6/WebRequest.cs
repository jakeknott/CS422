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
		private string _httpVersion;
		private string _URI;
		private string _method;

		public string URI{
			get{ return _URI;}
		}

		public string HttpVersion{
			get{ return _httpVersion;}
		}

		public string Method{
			get{ return _method;}
		}

		public long BodyLength {
			get {return _catStream.Length; }
		}

		public WebRequest (
			NetworkStream netStream, Stream catStream, string httpVersion, string URI, 
			string method)
		{			
			_netStream = netStream;
			_catStream = catStream;
			_httpVersion = httpVersion;
			_URI = URI;
			_method = method;
		}

		public void WriteNotFoundResponse(string pageHTML)
		{
			string respoString = _httpVersion + " 404 Not Found\r\n"
			                     + "Content-Length: " + pageHTML.Length
			                     + "Content-Type: text/html"
			                     + "\r\n\r\n"
			                     + pageHTML;


			byte[] response = Encoding.ASCII.GetBytes(respoString);

			_netStream.Write (response, 0, response.Length);

			
		}

		public bool WriteHTMLResponse(string htmlString)
		{
			string respoString = _httpVersion + " 200 OK\r\n"
			                     + "Content-Length: " + htmlString.Length
			                     + "Content-Type: text/html"
			                     + "\r\n\r\n"
			                     + htmlString;

			byte[] response = Encoding.ASCII.GetBytes(respoString);

			_netStream.Write (response, 0, response.Length);

			return true;
		}
	}
}

