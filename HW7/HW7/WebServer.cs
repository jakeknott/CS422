using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Collections.Concurrent;


namespace CS422
{
	public class WebServer : IThreadPool
	{
		private static ThreadPool pool;
		private  TcpClient _client; 
		private static BlockingCollection<WebService> services = new BlockingCollection<WebService>{ };
		private ValidateRequest validate = new ValidateRequest ();
		private static System.Threading.Thread listenThread;

		public WebServer(TcpClient client)
		{
			_client = client;
		}

		public WebServer (){}

		/// <summary>
		/// Start the specified port using the responseTemplate to write to it.
		/// </summary>
		/// <param name="port">Port.</param>
		/// <param name="responseTemplate">Response template.</param>
		public static void Start (Int32 port, Int32 numThreads)
		{
			pool = new ThreadPool (numThreads);

			listenThread = new System.Threading.Thread (Listen);
			listenThread.Start (port);
		}

		public static void Stop()
		{
			// wait for all threads in the thread pool to stop being active
			while(pool.ActiveThreads != 0)
			{}

			pool.Dispose ();
			listenThread.Abort ();
		}

		private static void Listen(object obj)
		{
			int port = (int)obj;
			TcpListener listener = new TcpListener(IPAddress.Any, port);
			TcpClient client;

			listener.Start();


			while(true)
			{
				// accept a client and start a thread with it. 
				client = listener.AcceptTcpClient();
				pool.Add (client);
			}
		}

		internal void AddService(WebService service)
		{
			services.Add (service);
		}

		public void ThreadWork()
		{
			WebRequest request = BuildRequest (_client);
			if (request == null)
			{
				_client.GetStream ().Dispose ();
				_client.Close ();
			}
			else
			{
				// we have a valid http request
				// Try to find a valid service 
				foreach(var service in services)
				{
					if ( validate.URI.StartsWith(service.ServiceURI))
					{
						service.Handler (request);
						break;
					}
					else
					{
						// No handler exists. 
						request.WriteNotFoundResponse ("Unable to find an appropriate handler");
					}
				}

				//Thread.Sleep (5000);
				_client.GetStream ().Dispose ();
				_client.Close ();
			}
		}

		private WebRequest BuildRequest(TcpClient client)
		{
			NetworkStream stream = client.GetStream();

			if (validate.IsValid(stream))
			{
				// This is the start of the body that we have already read into our buff.
				Stream memoryStream = new MemoryStream (validate.Buf, validate.Index, validate.BytesInBuf - validate.Index);

				// since the network stream cannot seek, we can just give it the stream and all 
				// reads will start from where we left off. (the part of
				string value;
				if (validate.Headers.TryGetValue ("content-length", out value))
				{
					int contentLength;

					if (!int.TryParse (value, out contentLength))
					{
						// we have the header, but it is not a valid int
						throw new ArgumentOutOfRangeException ("Invalid Content-Length header value");
					}

					// Here, we have the header, and a valid int, pass it to the concateStream
					ConcatStream catStream = new ConcatStream (memoryStream, stream, contentLength);
					return new WebRequest (stream, catStream, validate.HttpVersion, validate.URI, validate.Method);
				}
				else
				{
					// Here, we did not find the header, do not give it a length.
					ConcatStream catStream = new ConcatStream (memoryStream, stream);
					return new WebRequest (stream, catStream, validate.HttpVersion, validate.URI, validate.Method);
				}


				
				//isValidHTTP = true;
				//string formatedString = string.Format (responseTemplate, "11398813", DateTime.Now, _uri);
				//stream.Write (Encoding.ASCII.GetBytes (formatedString), 0, formatedString.Length);
			}


			//listener.Stop ();

			return null;
		}


	}
}

