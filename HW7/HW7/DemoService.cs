using System;

namespace CS422
{
	internal class DemoService : WebService
	{
		private const string c_template =
			"<html>This is the response to the request:<br>" +
			"Method: {0}<br>Request-Target/URI: {1}<br>" +
			"Request body size, in bytes: {2}<br><br>" +
			"Student ID: {3}</html>";

		public DemoService ()
		{
		}

		public override void Handler (WebRequest req)
		{
			string bodyLength;
			try
			{
				// If the req had a content length header,
				// this will work
				bodyLength = req.BodyLength.ToString ();
			}
			catch
			{
				// otherwise, it will throw an exception because
				// we do not know the length. 
				bodyLength = "Undefined";
			}

			req.WriteHTMLResponse (string.Format (c_template, req.Method, req.URI, bodyLength, "11398813"));
		}

		public override string ServiceURI {
			get
			{
				return "/";
			}
		}
	}
}

