using System;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Collections.Generic;

namespace CS422
{
	public abstract class Dir422
	{
		public abstract string Name {get;}

		public abstract Dir422 Parent { get;}

		public abstract IList<Dir422> GetDirs();

		public abstract IList<File422> GetFiles();

		public abstract bool ContainsFile (string fileName, bool reursive);

		public abstract bool ContainsDir(string dirName, bool recursive);

		public abstract Dir422 GetDir(string name);

		public abstract File422 GetFile (string name);

		public abstract File422 CreateFile(string name);

		public abstract Dir422 CreateDire(string name);
	}


	public abstract class File422
	{
		public abstract string Name { get; }

		public abstract Dir422 Parent {get;}

		// Stream returned better not allow writing.
		public abstract Stream OpenReadOnly();

		public abstract Stream OpenReadWrite();
	}

	public abstract class FileSys422
	{
		public abstract Dir422 GetRoot();

		public virtual bool Contains(File422 file)
		{
			return Contains (file.Parent);
		}

		public virtual bool Contains(Dir422 dir)
		{
			if (dir == null)
			{
				return false;
			}

			if (dir == GetRoot ())
			{
				return true;
			}

			return Contains (dir.Parent);
			
		}


	}

	public class StdFSDir: Dir422
	{
		private string m_path;

		public StdFSDir (string path)
		{
			m_path = path;
		}

		public override IList<File422> getFiles()
		{
			List<File422> files = new List<File422> ();
			foreach (string file in Directory.GetFiles (m_path))
			{
				files.Add (new StdFSDir (file));
			}

			return files;
		}
			
	}

	public class StdFSFile: File422
	{
		private string m_path;

		public StdFSFile (string path)
		{
			m_path = path;
		}

		public Stream OpenReadOnly()
		{
			
		}

	}


}
