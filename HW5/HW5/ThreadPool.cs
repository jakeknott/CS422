using System;
using System.IO;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Net.Sockets;


namespace CS422
{
	interface IThreadPool
	{
		void ThreadWork()	;
	}

	public class ThreadPool : IDisposable
	{
		private BlockingCollection<IThreadPool> coll;
		private List<Thread> allThreads;

		public ThreadPool ()
		{}

		public ThreadPool(ushort threadCount)
		{
			allThreads = new List<Thread>{ };

			coll = new BlockingCollection<IThreadPool>();

			if (threadCount <= 0)
			{
				threadCount = 64;
			}

			for (int i = 0; i < threadCount; i++)
			{
				Thread t = new Thread (ThreadWorkFunc);
				t.Start (coll);
				allThreads.Add (t);
			}
		}

		public void Add(TcpClient client)
		{
			WebServer server = new WebServer (client);
			coll.Add (server);
			/*
			foreach (var item in values)
			{
				ThreadPool sleeper = new ThreadPool (item, _writter);
				coll.Add (sleeper);
			}*/
		}

		// ThreadWorkFunc logic:
		public void ThreadWorkFunc(object blockColl) 
		{
			BlockingCollection<IThreadPool> collection = (BlockingCollection<IThreadPool>)blockColl;
			while (true)
			{		
				var task = collection.Take ();
				if (task == null)
				{
					break;
				}
				else
				{
					//handle
					task.ThreadWork ();
				}
			}
		}

		public void Dispose ()
		{
			foreach (var thread in allThreads)
			{
				thread.Abort ();
			}
		}
	}
}