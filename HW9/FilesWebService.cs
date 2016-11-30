using System;
using System.IO;
using System.Text;

namespace CS422
{
	internal class FilesWebService : WebService
	{
		private readonly FileSys422 r_sys;

		public FilesWebService (FileSys422 fs)
		{
			r_sys = fs;
		}

		public override void Handler (WebRequest req)
		{
			if (!req.URI.StartsWith (this.ServiceURI))
			{
				throw new InvalidOperationException ();
			}

			// Percent-decode our filename
			string percenDecoded = Uri.UnescapeDataString(req.URI);

			//If we want the root folder
			if (percenDecoded == ServiceURI)
			{
				RespondWithList (r_sys.GetRoot (), req);
				return;
			}

			// The pluse one is the '/' after the '/files' so we remove '/files/'
			string[] peices = percenDecoded.Substring (ServiceURI.Length + 1).Split ('/');

			Dir422 dir = r_sys.GetRoot ();
			for(int i = 0; i < peices.Length -1; i++)
			{
				dir = dir.GetDir (peices[i]);

				if (dir == null)
				{
					//This wants a response?
					req.WriteNotFoundResponse (string.Empty);
					return;
				}
			}

			string lastPeice = peices [peices.Length - 1];


			File422 file = dir.GetFile (lastPeice);
			if (file != null)
			{
				//Send back the file to them.
				RespondWithFile (file, req);
			}
			else
			{
				//Send the dir contents (if it is a dir to send back)
				dir = dir.GetDir (lastPeice);
				if (dir != null)
				{
					//Respond with the list of files and dirs
					RespondWithList (dir, req);
				}
				else
				{
					req.WriteNotFoundResponse (string.Empty);
				}

			}
		}

		private void RespondWithFile(File422 file, WebRequest req)
		{
			Stream fileStream = file.OpenReadOnly ();

			if (req.Headers.ContainsKey ("range"))
			{
				req.WritePartialDataResponse (fileStream, GetFileTypeHeader (file));
				fileStream.Close ();
				return;
			}


			req.WriteGenericFileResponse (fileStream, GetFileTypeHeader (file));

			fileStream.Close ();
		}

		private string GetFileTypeHeader(File422 file)
		{
			string[] split = file.Name.Split ('.');
			string type = split [split.Length - 1];

			//JPEG, PNG, PDF, MP4, TXT, HTML and XML 
			switch (type.ToLower ())
			{
			case "jpeg":
				return "image/jpeg";
			case "jpg":
				return "image/jpg";
			case "png":
				return "image/png";
			case "pdf":
				return "application/pdf";
			case "mp4":
				return "video/mp4";
			case "txt":
				return "text/plain";
			case "html":
				return "text/html";
			case "xml":
				return "text/xml";
			default:
				return null;
			}
		}

		private void RespondWithList(Dir422 dir, WebRequest req)
		{
			string html = BuildDirHTML (dir);
			req.WriteHTMLResponse (html);
		}

		private string BuildDirHTML(Dir422 directory)
		{
			var html = new System.Text.StringBuilder("<html>");

			html.Append ("<h1>Files</h1>");

			foreach(File422 file in directory.GetFiles())
			{
				string path = GetPath (file.Parent);
				path += "/" + file.Name;

				// percent incode from our filename
				path =  Uri.EscapeUriString(path);

				html.AppendFormat (
					"<a href=\"{0}\">{1}</a>",
					path, file.Name
				);
				html.Append ("<br>");
			}

			html.Append ("<h1>Directories</h1>");

			foreach (Dir422 dir in directory.GetDirs())
			{
				string path = GetPath (dir);

				html.AppendFormat (
					"<a href=\"{0}\">{1}</a>",
					path, dir.Name
				);

				html.Append ("<br>");
			}



			html.AppendLine ("</html>");

			return html.ToString ();
		}

		private string GetPath (Dir422 dir)
		{
			if (Equals(dir.Parent.Name, string.Empty))
			{
				return "/files";
			}

			return GetPath (dir.Parent) + "/" + dir.Name;
		}

		public override string ServiceURI 
		{
			get
			{
				return "/files";
			}
		}
	}
}

