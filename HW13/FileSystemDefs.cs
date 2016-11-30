using System;
using System.IO;
using System.Collections.Generic;

namespace CS422
{
	public abstract class Dir422
	{
		public abstract string Name {get;}

		public abstract IList<Dir422> GetDirs();

		public abstract IList<File422> GetFiles();

		public abstract Dir422 Parent { get;}

		public abstract bool ContainsFile (string fileName, bool recursive);

		public abstract bool ContainsDir(string dirName, bool recursive);

		public abstract Dir422 GetDir(string name);

		public abstract File422 GetFile (string name);

		public abstract File422 CreateFile(string name);

		public abstract Dir422 CreateDir(string name);
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
		public abstract Dir422 GetRoot ();

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

			if (dir ==  GetRoot ())
			{
				return true;
			}

			return Contains (dir.Parent);
		}
	}

	public class StdFSDir: Dir422
	{
		private string m_path;
		private string m_name;
		private static bool _isRoot;

		//Defualt is not the root
		public StdFSDir (string path)
		{
			m_path = path;
			m_name = Path.GetFileName (path);
			_isRoot = false;
		}

		//If we need to set isRoot then do that here.
		public StdFSDir (string path, bool isRoot)
		{
			m_path = path;
			m_name = Path.GetFileName (path);
			_isRoot = isRoot;

		}

		private bool ValidateName(string name)
		{
			if (name.Contains ("/") || name.Contains ("\\")
				|| name == null || name == string.Empty)
			{
				return false;
			}

			return true;
		}

		public override string Name 
		{
			get { return m_name;}
		}


		public override IList<File422> GetFiles()
		{
			List<File422> files = new List<File422> ();
			foreach (string file in Directory.GetFiles (m_path))
			{
				files.Add(new StdFSFile(file));
			}

			return files;
		}
	
		public override IList<Dir422> GetDirs()
		{
			List<Dir422> dirs = new List<Dir422> ();
			foreach (string dir in Directory.GetDirectories (m_path))
			{
				dirs.Add(new StdFSDir(dir));
			}

			return dirs;
		}

		public override Dir422 Parent
		{ 
			get 
			{
				if (_isRoot)
				{
					return null;
				}
				else
				{
					return new StdFSDir (Directory.GetParent (m_path).FullName);
				}
			}
		}

		public override bool ContainsFile (string fileName, bool recursive)
		{
			if (!ValidateName (fileName))
			{
				return false;
			}

			if (recursive)
			{
				return ContainsFileRecursive (fileName, this);
			}

			IList<File422> files = GetFiles ();

			foreach(File422 file in files)
			{
				if (file.Name == fileName)
				{
					return true;
				}
			}

			return false;
		}

		private bool ContainsFileRecursive(string fileName, Dir422 dir)
		{
			IList<File422> files = dir.GetFiles ();

			foreach(File422 file in files)
			{
				if (file.Name == fileName)
				{
					return true;
				}
			}

			// Get the current dir's subdirs
			IList<Dir422> subDirs = dir.GetDirs ();

			//Here we have looked at all the files in our directory and did nt
			// find our filename, now we need to look in a subdirectory
			foreach (Dir422 subDir in subDirs)
			{
				if (ContainsFileRecursive (fileName, subDir))
				{
					return true;
				}
			}

			return false;
		}

		public override bool ContainsDir(string dirName, bool recursive)
		{
			if (!ValidateName (dirName))
			{
				return false;
			}

			if (recursive)
			{
				return ContainsDirRecursive (dirName, this);
			}

			// If not recursive then just look at immediate subdirs
			IList<Dir422> subDirs = GetDirs ();

			foreach(Dir422 subDir in subDirs)
			{
				if(subDir.Name == dirName)
				{
					return true;
				}
			}

			return false;
		}

		private bool ContainsDirRecursive(string dirName, Dir422 dir)
		{
			if (dir.Name == dirName)
			{
				return true;
			}
			
			// Get the current dir's subdirs
			IList<Dir422> subDirs = dir.GetDirs ();

			foreach(Dir422 subDir in subDirs)
			{
				if(ContainsDirRecursive (dirName, subDir))
				{
					return true;
				}
			}

			return false;
		}

		public override Dir422 GetDir(string name)
		{
			if (!ValidateName (name))
			{
				return null;
			}

			IList<Dir422> subDirs = GetDirs ();

			foreach(Dir422 subDir in subDirs)
			{
				if(subDir.Name == name)
				{
					return subDir;
				}
			}

			return null;
		}

		public override File422 GetFile (string name)
		{
			if (!ValidateName (name))
			{
				return null;
			}

			IList<File422> files = GetFiles ();

			foreach(File422 file in files)
			{
				if(file.Name == name)
				{
					return file;
				}
			}

			return null;
		}

		public override File422 CreateFile(string name)
		{
			if (!ValidateName (name))
			{
				return null;
			}

			string newFilePath = Path.Combine (m_path, name);

			// If the file exist delete it, this is to ensure that if others have the
			// file open there is no problems.
			if (File.Exists (newFilePath))
			{
				// If the file exists then we dont want to upload it again.
				return null;
			}

			//Since create returns a stream, we need to capture it and close it.
			Stream s = File.Create (newFilePath);
			s.Close ();

			return new StdFSFile(newFilePath);
		}

		public override Dir422 CreateDir(string name)
		{
			if (!ValidateName (name))
			{
				return null;
			}

			string newDirPath = Path.Combine (m_path, name);

			if (Directory.Exists (newDirPath))
			{
				return new StdFSDir (newDirPath);
			}

			// The file does not exists, crete it
			Directory.CreateDirectory (newDirPath);

			return new StdFSDir (newDirPath);
		}

	}

	public class StdFSFile: File422
	{
		private string m_path;
		private string m_name; 

		public StdFSFile (string path)
		{
			m_path = path;
			m_name = Path.GetFileName (path);
		}

		public override string Name
		{
			get {return m_name; } 
		}

		public override Dir422 Parent 
		{
			get{ return new StdFSDir (Directory.GetParent (m_path).FullName);}
		}

		// Stream returned better not allow writing.
		public override Stream OpenReadOnly()
		{
			try
			{
				return File.Open (m_path, FileMode.Open, FileAccess.Read);
			}
			catch
			{
				return null;
			}
		}

		public override Stream OpenReadWrite()
		{
			try
			{
				return File.Open (m_path, FileMode.Open, FileAccess.ReadWrite);
			}
			catch
			{
				return null;
			}
		}
	}

	public class StandardFileSystem: FileSys422
	{
		private Dir422 m_rootDir;

		private StandardFileSystem (string rootDir)
		{
			m_rootDir = new StdFSDir ( rootDir, true);
		}

		public static StandardFileSystem Create(string rootDir)
		{
			if (Directory.Exists (rootDir))
			{
				return new StandardFileSystem (rootDir);
			}

			return null;
		}

		public override Dir422 GetRoot ()
		{
			return m_rootDir;
		}
	}

	public class MemoryFileSystem : FileSys422
	{
		private Dir422 root;
		public MemoryFileSystem()
		{
			root = new MemFSDir ("root");
		}

		public override Dir422 GetRoot ()
		{
			return root;
		}
	}

	public class MemFSDir : Dir422
	{
		Dir422 parent;
		List<Dir422> dirs;
		List<File422> files;
		string name;

		public MemFSDir(string Name)
		{
			dirs = new List<Dir422> ();
			files = new List<File422> ();
			name = Name;
			parent = null;
		}

		public MemFSDir (Dir422 Parent, string Name)
		{
			dirs = new List<Dir422> ();
			files = new List<File422> ();
			parent = Parent;
			name = Name;
		}

		private bool ValidateName(string name)
		{
			if (name.Contains ("/") || name.Contains ("\\")
				|| name == null || name == string.Empty)
			{
				return false;
			}

			return true;
		}

		public override string Name 
		{
			get {return name;}
		}

		public override IList<Dir422> GetDirs()
		{
			return dirs;
		}
	

		public override IList<File422> GetFiles()
		{
			return files;
		}

		public override Dir422 Parent 
		{ 
			get {return parent; }
		}

		public override bool ContainsFile (string fileName, bool recursive)
		{
			if (!ValidateName (fileName))
			{
				return false;
			}

			if (recursive)
			{
				return ContainsFileRecursive (fileName, this);
			}

			foreach(File422 file in files)
			{
				if (file.Name == fileName)
				{
					return true;
				}
			}

			return false;
		}

		private bool ContainsFileRecursive(string fileName, Dir422 dir)
		{
			IList<File422> files = dir.GetFiles ();

			foreach(File422 file in files)
			{
				if (file.Name == fileName)
				{
					return true;
				}
			}

			// Get the current dir's subdirs
			IList<Dir422> subDirs = dir.GetDirs ();

			//Here we have looked at all the files in our directory and did nt
			// find our filename, now we need to look in a subdirectory
			foreach (Dir422 subDir in subDirs)
			{
				if (ContainsFileRecursive (fileName, subDir))
				{
					return true;
				}
			}

			return false;
		}

		public override bool ContainsDir(string dirName, bool recursive)
		{
			if (!ValidateName (dirName))
			{
				return false;
			}

			if (recursive)
			{
				return ContainsDirRecursive (dirName, this);
			}

			foreach(Dir422 dir in dirs)
			{
				if (dir.Name == dirName)
				{
					return true;
				}
			}

			return false;
		}

		private bool ContainsDirRecursive (string dirName, Dir422 dir)
		{
			if (dir.Name == dirName)
			{
				return true;
			}

			// Get the current dir's subdirs
			IList<Dir422> subDirs = dir.GetDirs ();

			foreach(Dir422 subDir in subDirs)
			{
				if(ContainsDirRecursive (dirName, subDir))
				{
					return true;
				}
			}

			return false;
		}

		public override Dir422 GetDir(string name)
		{
			if (!ValidateName (name))
			{
				return null;
			}

			foreach (Dir422 dir in dirs)
			{
				if (dir.Name == name)
				{
					return dir;
				}
			}

			// If we did not find our desired dir
			return null;
		}

		public override File422 GetFile (string name)
		{
			if (!ValidateName (name))
			{
				return null;
			}

			foreach (File422 file in files)
			{
			if (file.Name == name)
				{
					return file;
				}
			}

			// If we did not find our desired file
			return null;
		}

		public override File422 CreateFile(string name)
		{
			if (!ValidateName (name))
			{
				return null;
			}
			
			File422 newFile = new MemFSFile (this, name);
			files.Add (newFile);
			return newFile;
		}

		public override Dir422 CreateDir(string name)
		{
			if (!ValidateName (name))
			{
				return null;
			}

			Dir422 newDir = new MemFSDir (this, name);
			dirs.Add (newDir);
			return newDir;
		}
	}

	public class MemFSFile: File422
	{
		string name;
		Dir422 parent;
		MyMemStream data;
		List<MyMemStream> openForRead;
		List<MyMemStream> openForWrite;

		public MemFSFile(Dir422 Parent, string Name)
		{
			name = Name;
			parent = Parent;
			data = new MyMemStream (true, true);
			openForRead = new List<MyMemStream> ();
			openForWrite = new List<MyMemStream> ();
		}

		public override string Name 
		{
			get { return name; } 
		}

		public override Dir422 Parent
		{
			get { return parent; }
		}

		private void TrimStreamLists ()
		{
			List<MyMemStream> toRemove = new List<MyMemStream> ();

			foreach(MyMemStream s in openForRead)
			{
				if (s.IsDisposed)
				{
					toRemove.Add (s);
				}
			}

			foreach (var del in toRemove)
			{
				openForRead.Remove (del);
			}

			toRemove = new List<MyMemStream> ();

			foreach(MyMemStream s in openForWrite)
			{
				if (s.IsDisposed)
				{
					toRemove.Add (s);
				}
			}

			foreach (var del in toRemove)
			{
				openForWrite.Remove (del);
			}
		}

		public override Stream OpenReadOnly()
		{
			//removing all strems that are not used anymore
			TrimStreamLists ();

			// If there is a stream open for write, we cannot open one for reading
			// othewise we can open for reading, even if there is already a stream open for read. 
			if (openForWrite.Count > 0)
			{
				return null;
			}

			MyMemStream s = new MyMemStream (data, true, false);
			//data.CopyTo (s);
			openForRead.Add (s);
			return s;
		}

		public override Stream OpenReadWrite()
		{
			//removing all strems that are not used anymore
			TrimStreamLists ();

			// If there are any streams open, wether that is for read or write, return null.
			if (openForWrite.Count > 0 || openForRead.Count > 0)
			{
				return null;
			}

			//When opening a file, open it at the beginning.
			data.Seek (0, SeekOrigin.Begin);
			openForWrite.Add (data);
			return data;
		}
	}

	public class MyMemStream: Stream
	{
		private Stream myStream;
		private bool isDisposed;
		private bool canRead;
		private bool canWrite;

		public MyMemStream(bool CanRead, bool CanWrite)
		{
			myStream = new MemoryStream ();
			isDisposed = false;
			canRead = CanRead;
			canWrite = CanWrite;
		}

		public MyMemStream(Stream s, bool CanRead, bool CanWrite)
		{
			myStream = new MemoryStream ();

			// in order to copy it starts at the current position
			s.Seek (0, SeekOrigin.Begin);

			//Copy the stream to our internal memory stream
			s.CopyTo (myStream);

			// Move our memory stream back to the start
			// This is now a differnet instance of the stream
			// and we can move in this one without effecting our 
			// original stream.
			myStream.Seek (0, SeekOrigin.Begin);
			isDisposed = false;
			canRead = CanRead;
			canWrite = CanWrite;
		}

		public bool IsDisposed 
		{
			get { return isDisposed; }
		}

		public override bool CanRead {
			get {
				return canRead;
			}
		}

		public override bool CanSeek {
			get {
				return myStream.CanSeek;
			}
		}

		public override bool CanWrite {
			get {
				return canWrite;
			}
		}

		public override long Length {
			get {
				return myStream.Length;
			}
		}

		public override long Position {
			get {
				return myStream.Position;
			}
			set {
				myStream.Position = value;
			}
		}

		public override void Flush ()
		{
			myStream.Flush ();
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			return myStream.Seek (offset, origin);
		}

		public override void SetLength (long value)
		{
			myStream.SetLength (value);
		}

		public override void Write (byte[] buffer, int offset, int count)
		{
			if (canWrite)
			{
				myStream.Write (buffer, offset, count);
			}
			else
			{
				throw new NotSupportedException ();
			}

		}

		public override int Read (byte[] buffer, int offset, int count)
		{
			if (canRead)
			{
				return myStream.Read (buffer, offset, count);
			}
			else
			{
				throw new NotSupportedException ();
			}
		}

		protected override void Dispose (bool disposing)
		{
			isDisposed = true;
			base.Dispose (disposing);
		}
	}


}
