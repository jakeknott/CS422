using System;
using System.Net.Sockets;
using System.Text;
using System.IO;

namespace CS422
{
	public class WebRequest
	{
		private NetworkStream _netStream; 
		Stream _catStream;
		private string _httpVersion;
		private string _URI;
		private string _method;

		public WebRequest (NetworkStream netStream, Stream catStream, string httpVersion, string URI, string method)
		{			
			_netStream = netStream;
			_catStream = catStream;
			_httpVersion = httpVersion;
			_URI = URI;
			_method = method;
		}

		public void WriteNotFoundResponse(string pageHTML)
		{
			string respoString = _httpVersion + " 404 Not Found\r\nContent-Type: text/html\r\nContent-Length: "
			                     + _catStream.Length
			                     + "\n\r\r\n"
			                     + pageHTML;


			byte[] response = Encoding.ASCII.GetBytes(respoString);

			_netStream.Write (response, 0, response.Length);

			
		}

		public bool WriteHTMLResponse(string htmlString)
		{
			string respoString = _httpVersion + " 404 Not Found\r\nContent-Type: text/html\r\nContent-Length: "
			                     + _catStream.Length
			                     + "\n\r\r\n"
			                     + htmlString;

			byte[] response = Encoding.ASCII.GetBytes(respoString);

			_netStream.Write (response, 0, response.Length);

			return true;
		}
	}
}

