using System;
using System.IO;
using System.Text;

namespace CS422
{
	internal class FilesWebService : WebService
	{
		private readonly FileSys422 r_sys;
		private bool m_allowUploads;

		public FilesWebService (FileSys422 fs)
		{
			r_sys = fs;
			m_allowUploads = true;
		}

		public override void Handler (WebRequest req)
		{
			if (!req.URI.StartsWith (this.ServiceURI))
			{
				throw new InvalidOperationException ();
			}

			// Percent-decode our filename
			string percenDecoded = Uri.UnescapeDataString(req.URI);

			if (req.Method == "PUT")
			{
				DoUpload (req, percenDecoded);
				return;
			}



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

		private void DoUpload(WebRequest req, string percenDecoded)
		{
			string fileName = "";

			Dir422 currentDir = null;
			if (percenDecoded == ServiceURI)
			{
				currentDir = r_sys.GetRoot ();
			}
			else
			{
				// The pluse one is the '/' after the '/files' so we remove '/files/'
				string[] pathPeices = percenDecoded.Substring (ServiceURI.Length + 1).Split ('/');

				currentDir = r_sys.GetRoot ();
				for(int i = 0; i < pathPeices.Length -1; i++)
				{
					currentDir = currentDir.GetDir (pathPeices[i]);

					if (currentDir == null)
					{
						req.WriteNotFoundResponse (string.Empty);
						return;
					}
				}

				fileName = pathPeices [pathPeices.Length - 1];
			}

			File422 newFile = currentDir.CreateFile (fileName);

			if (newFile == null)
			{
				// We could not create a new file
				return;
			}

			Stream s = newFile.OpenReadWrite ();

			if (s == null)
			{
				return;
			}

			byte[] bodyBytes = new byte[1024];

			long bytesRead = 0;

			//Read all that we can.
			while (bytesRead < req.BodyLength)
			{
				// We need to read chuncks at a time since read wants an int 
				// as the count, and we may loose data if we cast a long to an int.
				int read = req.Body.Read (bodyBytes, 0, 1024);
				bytesRead += read;
				if (read == 0)
				{
					break;
				}

				s.Write (bodyBytes, 0, read);
				Array.Clear (bodyBytes, 0, bodyBytes.Length);
			}
				
			s.Close ();

			req.WriteHTMLResponse ("");
			return;
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

			// We'll need a bit of script if uploading is allowed
			if (m_allowUploads)
			{
				html.AppendLine(
					@"<script>
						function selectedFileChanged(fileInput, urlPrefix)
						{
							 document.getElementById('uploadHdr').innerText = 'Uploading ' + fileInput.files[0].name + '...';
							 // Need XMLHttpRequest to do the upload
							 if (!window.XMLHttpRequest)
							 {
								 alert('Your browser does not support XMLHttpRequest. Please update your browser.');
								 return;
							 }
							 // Hide the file selection controls while we upload
							 var uploadControl = document.getElementById('uploader');
							 if (uploadControl)
							 {
							 	uploadControl.style.visibility = 'hidden';
							 }
							 // Build a URL for the request
							 if (urlPrefix.lastIndexOf('/') != urlPrefix.length - 1)
							 {
							 	urlPrefix += '/';
							 }

							 var uploadURL = urlPrefix + fileInput.files[0].name;
							 // Create the service request object
							 var req = new XMLHttpRequest();
							 req.open('PUT', uploadURL);
							 req.onreadystatechange = function()
							 {
							 	document.getElementById('uploadHdr').innerText = 'Upload (request status == ' + req.status + ')';
							 };
							 req.send(fileInput.files[0]);
						}
					</script>
				");
			}

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


			if (m_allowUploads)
			{
				html.AppendFormat(
					"<hr><h3 id='uploadHdr'>Upload</h3><br>" +
					"<input id=\"uploader\" type='file' " +
					"onchange='selectedFileChanged(this,\"{0}\")' /><hr>",
					GetPath(directory));
			}


			html.AppendLine ("</html>");

			return html.ToString ();
		}

		private string GetPath (Dir422 dir)
		{
			if (dir.Parent == null)
			{
				return "";
			}

			if (Equals(dir.Parent.Name, string.Empty)  || dir.Name == r_sys.GetRoot ().Name)
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

