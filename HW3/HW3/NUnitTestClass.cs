using NUnit.Framework;
using System;
using System.IO;
using System.Text;

namespace CS422
{
	[TestFixture ()]
	public class NUnitTestClass
	{
		[Test ()]
		public void TestRequestLine ()
		{
			string requestLine = "GET FDFDadf HTTP/1.1\r\n";
			byte[] byteArray = Encoding.UTF8.GetBytes(requestLine);
			//byte[] byteArray = Encoding.ASCII.GetBytes(contents);
			MemoryStream stream = new MemoryStream(byteArray);

			Assert.IsTrue (WebServer.IsValidRequestLine (stream));

			requestLine = "AET FDFDadf HTTP/1.1\r\n";
			byteArray = Encoding.UTF8.GetBytes(requestLine);
			//byte[] byteArray = Encoding.ASCII.GetBytes(contents);
			stream = new MemoryStream(byteArray);

			Assert.IsFalse (WebServer.IsValidRequestLine (stream));

			requestLine = "GET FDFDadf HTTP/2\r\n";
			byteArray = Encoding.UTF8.GetBytes(requestLine);
			//byte[] byteArray = Encoding.ASCII.GetBytes(contents);
			stream = new MemoryStream(byteArray);

			Assert.IsFalse (WebServer.IsValidRequestLine (stream));

			requestLine = "GET FDFDadfHTTP/2\r\n";
			byteArray = Encoding.UTF8.GetBytes(requestLine);
			//byte[] byteArray = Encoding.ASCII.GetBytes(contents);
			stream = new MemoryStream(byteArray);

			Assert.IsFalse (WebServer.IsValidRequestLine (stream));


			requestLine = "GET FDFDadfHTTP/2\r";
			byteArray = Encoding.UTF8.GetBytes(requestLine);
			//byte[] byteArray = Encoding.ASCII.GetBytes(contents);
			stream = new MemoryStream(byteArray);

			Assert.IsFalse (WebServer.IsValidRequestLine (stream));

			requestLine = "GETFDFDadf HTTP/2\r";
			byteArray = Encoding.UTF8.GetBytes(requestLine);
			//byte[] byteArray = Encoding.ASCII.GetBytes(contents);
			stream = new MemoryStream(byteArray);

			Assert.IsFalse (WebServer.IsValidRequestLine (stream));

			requestLine = "GET  FDFDadf HTTP/1.1\r";
			byteArray = Encoding.UTF8.GetBytes(requestLine);
			//byte[] byteArray = Encoding.ASCII.GetBytes(contents);
			stream = new MemoryStream(byteArray);

			Assert.IsFalse (WebServer.IsValidRequestLine (stream));

			requestLine = "GET \tFDFDadf HTTP/1.1\r";
			byteArray = Encoding.UTF8.GetBytes(requestLine);
			//byte[] byteArray = Encoding.ASCII.GetBytes(contents);
			stream = new MemoryStream(byteArray);

			Assert.IsFalse (WebServer.IsValidRequestLine (stream));

			requestLine = "GET\tFDFDadf HTTP/1.1\r";
			byteArray = Encoding.UTF8.GetBytes(requestLine);
			//byte[] byteArray = Encoding.ASCII.GetBytes(contents);
			stream = new MemoryStream(byteArray);

			Assert.IsFalse (WebServer.IsValidRequestLine (stream));
		}

		[Test]
		public void TestHTTPHeaders()
		{
			string requestLine = "HeaderFejils: dfdf::jfjek:\r\n\r\n";
			byte[] byteArray = Encoding.UTF8.GetBytes(requestLine);
			//byte[] byteArray = Encoding.ASCII.GetBytes(contents);
			MemoryStream stream = new MemoryStream(byteArray);

			Assert.IsTrue (WebServer.IsValidHeader (stream));


			requestLine = "HeaderFejils : dfdf::jfjek:\r\n\r\n";
			byteArray = Encoding.UTF8.GetBytes(requestLine);
			//byte[] byteArray = Encoding.ASCII.GetBytes(contents);
			stream = new MemoryStream(byteArray);

			Assert.IsFalse (WebServer.IsValidHeader (stream));

			requestLine = "HeaderFejils\t: dfdf::jfjek:\r\n\r\n";
			byteArray = Encoding.UTF8.GetBytes(requestLine);
			//byte[] byteArray = Encoding.ASCII.GetBytes(contents);
			stream = new MemoryStream(byteArray);

			Assert.IsFalse (WebServer.IsValidHeader (stream));

			requestLine = "HeaderFejils\t:dfdf::jfjek:\r\n\r\n";
			byteArray = Encoding.UTF8.GetBytes(requestLine);
			//byte[] byteArray = Encoding.ASCII.GetBytes(contents);
			stream = new MemoryStream(byteArray);

			Assert.IsFalse (WebServer.IsValidHeader (stream));

			requestLine = ":HeaderFejils:dfdf::jfjek:\r\n\r\n";
			byteArray = Encoding.UTF8.GetBytes(requestLine);
			//byte[] byteArray = Encoding.ASCII.GetBytes(contents);
			stream = new MemoryStream(byteArray);

			Assert.IsFalse (WebServer.IsValidHeader (stream));

			requestLine = " HeaderFejils:dfdf::jfjek:\r\n\r\n";
			byteArray = Encoding.UTF8.GetBytes(requestLine);
			//byte[] byteArray = Encoding.ASCII.GetBytes(contents);
			stream = new MemoryStream(byteArray);

			Assert.IsFalse (WebServer.IsValidHeader (stream));
		}

		[Test]
		public void TestIsValid()
		{
			string requestLine = "GET FDFDadf HTTP/1.1\r\nHeaderFejils:dfdf::jfjek:\r\n\r\n";
			byte[] byteArray = Encoding.UTF8.GetBytes(requestLine);
			//byte[] byteArray = Encoding.ASCII.GetBytes(contents);
			MemoryStream stream = new MemoryStream(byteArray);

			Assert.IsTrue (WebServer.IsVaid(stream));

			requestLine = "GET FDFDadf HTTP/1.1\r\n HeaderFejils:dfdf::jfjek:\r\n\r\n";
			byteArray = Encoding.UTF8.GetBytes(requestLine);
			//byte[] byteArray = Encoding.ASCII.GetBytes(contents);
			stream = new MemoryStream(byteArray);

			Assert.IsFalse (WebServer.IsVaid(stream));

			requestLine = "GET FDFDadf HTTP/1.1HeaderFejils\t:dfdf::jfjek:\r\n\r\n";
			byteArray = Encoding.UTF8.GetBytes(requestLine);
			//byte[] byteArray = Encoding.ASCII.GetBytes(contents);
			stream = new MemoryStream(byteArray);

			Assert.IsFalse (WebServer.IsVaid(stream));
		}
	}
}

