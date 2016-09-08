using System;
using System.IO;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;


namespace CS422
{
	public class ThreadPoolSleepSorter : IDisposable
	{
		private TextWriter _writter;
		private BlockingCollection<ThreadPoolSleepSorter> coll;
		private byte _sleepTime;
		private List<Thread> allThreads;

		private ThreadPoolSleepSorter()
		{}

		private ThreadPoolSleepSorter(byte sleepTime, TextWriter output)
		{
			_sleepTime = sleepTime;
			_writter = output;
		}

		public ThreadPoolSleepSorter(TextWriter output, ushort threadCount)
		{
			allThreads = new List<Thread>{ };

			coll = new BlockingCollection<ThreadPoolSleepSorter>();
			_writter = output;

			if (threadCount == 0)
			{
				threadCount = 64;
			}

			for (int i = 0; i < threadCount; i++)
			{
				ThreadPoolSleepSorter sleeper = new ThreadPoolSleepSorter ();
				Thread t = new Thread (sleeper.ThreadWorkFunc);
				t.Start (coll);
				allThreads.Add (t);
			}
		}

		public void Sort(byte[] values)
		{
			foreach (var item in values)
			{
				ThreadPoolSleepSorter sleeper = new ThreadPoolSleepSorter (item, _writter);
				coll.Add (sleeper);
			}
		}

		// ThreadWorkFunc logic:
		public void ThreadWorkFunc(object blockColl) 
		{
			BlockingCollection<ThreadPoolSleepSorter> collection = (BlockingCollection<ThreadPoolSleepSorter>)blockColl;
			while (true)
			{				
				collection.Take ().Sleep ();
			}
		}

		public void Sleep()
		{
			Thread.Sleep (_sleepTime * 1000);
			_writter.WriteLine (_sleepTime);
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