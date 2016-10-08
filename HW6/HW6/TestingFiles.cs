using NUnit.Framework;
using System;

namespace CS422
{
	[TestFixture ()]
	public class TestingFiles
	{
		[Test ()]
		public void TestCase ()
		{
			
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

