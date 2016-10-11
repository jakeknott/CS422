using NUnit.Framework;
using System;
using System.IO;
using System.Text;

namespace CS422
{
	[TestFixture ()]
	public class TestingFiles
	{
		[Test ()]
		public void TestCase ()
		{
			string requestLine = "GET ";   

			for (int i = 0; i < 2048; i++)
			{
				requestLine += "a";
			}

			requestLine += "HTTP/1.1\r\n\r\n";

			byte[] byteArray = Encoding.UTF8.GetBytes(requestLine);
			//byte[] byteArray = Encoding.ASCII.GetBytes(contents);
			MemoryStream stream = new MemoryStream(byteArray);
			ValidateRequest validate = new ValidateRequest ();

			Assert.IsFalse (validate.IsValid (stream));

			requestLine = "GET URL HTTP/1.1\r\n";   // this is valid

			for (int i = 0; i < 100 * 1024; i++)
			{
				requestLine += "a:"; //adding a lot of giberish for the 'headers'
			}

			byteArray = Encoding.UTF8.GetBytes(requestLine);
			stream = new MemoryStream(byteArray);
			validate = new ValidateRequest ();
			Assert.IsFalse (validate.IsValid (stream));


			
			WebServer server = new WebServer ();
			WebServer.Start (4422, 0);
			WebService service = new DemoService ();

			server.AddService (service);

			while(true)
			{}


			WebServer.Stop ();
		}
	}
}

