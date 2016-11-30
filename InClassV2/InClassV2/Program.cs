using System;

namespace CS422
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			WebServer.Start (4220, 1);
			FilesWebService fileService = new FilesWebService(StandardFileSystem.Create ("/home/jakeknott/Downloads"));
			WebServer.AddService (fileService);

			while(true)
			{
				
			}
		}
	}
}
