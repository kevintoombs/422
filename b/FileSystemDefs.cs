using System;
using System.Collections.Generic;
using System.IO;

namespace CS422
{
    public abstract class Dir422
    {
        public abstract string Name { get; }
        public abstract IList<Dir422> GetDirs();
        public abstract IList<File422> GetFiles();
        public abstract Dir422 Parent { get; }
        public abstract bool ContainsFile(string fileName, bool recursive);
        public abstract bool ContainsDir(string dirName, bool recursive);
        public abstract File422 GetFile(string fileName);
        public abstract Dir422 GetDir(string dirName);
        public abstract File422 CreateFile(string fileName);
        public abstract Dir422 CreateDir(string dirName);
    }

    public abstract class File422
    {
        public abstract Dir422 Parent { get; }
        public abstract string Name { get; }
        public abstract Stream OpenReadOnly();
        public abstract Stream OpenReadWrite();
    }

    public abstract class FileSys422
    {
        public abstract Dir422 GetRoot();
        public virtual bool Contains(File422 file)
        {
            return Contains(file.Parent);
        }

        public virtual bool Contains(Dir422 directory)
        {
            if (ReferenceEquals(directory.Parent, GetRoot())) return true;
            if (directory.Parent == null) return false;
            return Contains(directory.Parent);
        }
    }

    public class StandardFileSystem : FileSys422
    {

        private StandardFileSystem(string rootDir)
        {
            Root = new StdFSDir(rootDir, null);
        }

        private StdFSDir Root { get; set; }

        public static StandardFileSystem Create(string rootDir)
        {
            bool exists = Directory.Exists(rootDir);
            if (!exists) return null;
            return new StandardFileSystem(rootDir);
        }

        public override Dir422 GetRoot()
        {
            return Root;
        }

    }

    public class StdFSDir : Dir422
    {
        internal StdFSDir(string path, Dir422 parent)
        {
            MyPath = path;
            Parent = parent;
        }

        private string MyPath { get; set; }

        public override string Name
        {
            get
            {
                return Path.GetFileName(MyPath);
            }
        }

        public override Dir422 Parent
        {
            get;
        }

        private bool ContainsPathChars(string dirName)
        {
            return (dirName.Contains("/") || dirName.Contains("\\"));
        }

        public override bool ContainsDir(string dirName, bool recursive)
        {
            if (ContainsPathChars(dirName)) return false;

            foreach(StdFSDir dir in GetDirs())
            {
                if (dir.Name == dirName) return true;
                if (recursive)
                {
                    if (dir.ContainsDir(dirName, true)) return true;
                }
            }

            return false;
        }

        public override bool ContainsFile(string fileName, bool recursive)
        {
            if (ContainsPathChars(fileName)) return false;

            foreach(StdFSFile file in GetFiles())
            {
                if (file.Name == fileName) return true;
            }

            if (recursive)
            {
                foreach (StdFSDir dir in GetDirs())
                {
                    if (dir.ContainsFile(fileName, true)) return true;
                }
            }

            return false;
        }

        public override Dir422 CreateDir(string dirName)
        {
            if (ContainsPathChars(dirName) || String.IsNullOrEmpty(dirName)) return null;
            if (ContainsDir(dirName, false)) return GetDir(dirName);

            string path = Path.Combine(MyPath, dirName);
            Directory.CreateDirectory(path);
            return new StdFSDir(path, this);
            throw new NotImplementedException();
        }


        public override File422 CreateFile(string fileName)
        {
            if (ContainsPathChars(fileName) || String.IsNullOrEmpty(fileName)) return null;

            string path = Path.Combine(MyPath, fileName);
            File.Create(path);
            return new StdFSFile(path, this);
        }

        public override Dir422 GetDir(string dirName)
        {
            if (ContainsPathChars(dirName)) return null;

            foreach (StdFSDir dir in GetDirs())
            {
                if (dir.Name == dirName) return dir;
            }

            return null;
        }

        public override IList<Dir422> GetDirs()
        {
            var dirStrings = Directory.GetDirectories(MyPath);
            List<Dir422> dirs = new List<Dir422>();
            foreach (var dir in dirStrings)
            {
                dirs.Add(new StdFSDir(dir, this));
            }
            return dirs;
        }

        public override File422 GetFile(string fileName)
        {
            if (ContainsPathChars(fileName)) return null;

            foreach (StdFSFile file in GetFiles())
            {
                if (file.Name == fileName) return file;
            }

            return null;
        }

        public override IList<File422> GetFiles()
        {
            var fileStrings = Directory.GetFiles(MyPath);
            List<File422> files = new List<File422>();
            foreach (var file in fileStrings)
            {
                files.Add(new StdFSFile(file, this));
            }
            return files;
        }
    }

    public class StdFSFile : File422
    {
        private string m_path;
        private StdFSDir m_parent;

        internal StdFSFile(string path, StdFSDir parent)
        {
            m_path = path;
            m_parent = parent;
        }

        public override string Name
        {
            get
            {
                return Path.GetFileName(m_path);
            }
        }

        public override Dir422 Parent
        {
            get
            {
                return m_parent;
            }
        }

        public override Stream OpenReadOnly()
        {
            return new FileStream(m_path, FileMode.Open, FileAccess.Read);
        }

        public override Stream OpenReadWrite()
        {
            try
            {
                return new FileStream(m_path, FileMode.Open, FileAccess.ReadWrite);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    public class MemoryFileSystem : FileSys422
    {
        private MemFSDir Root { get; set; }

        public MemoryFileSystem()
        {
            Root = new MemFSDir("/", null);
        }

        public override Dir422 GetRoot()
        {
            return Root;
        }
    }

    public class MemFSDir : Dir422
    {

        public List<Dir422> Dirs { get; private set; }
        public List<File422> Files { get; private set; }

        public MemFSDir(string name, MemFSDir parent)
        {
            Name = name;
            Parent = parent;

            Files = new List<File422>();
            Dirs = new List<Dir422>();
        }

        public override string Name
        {
            get;
        }

        public override Dir422 Parent
        {
            get;
        }

        private bool ContainsPathChars(string dirName)
        {
            return (dirName.Contains("/") || dirName.Contains("\\"));
        }

        public override bool ContainsDir(string dirName, bool recursive)
        {
            if (ContainsPathChars(dirName)) return false;

            foreach (MemFSDir dir in GetDirs())
            {
                if (dir.Name == dirName) return true;
                if (recursive)
                {
                    if (dir.ContainsDir(dirName, true)) return true;
                }
            }

            return false;
        }

        public override bool ContainsFile(string fileName, bool recursive)
        {
            if (ContainsPathChars(fileName)) return false;

            foreach (MemFSFile file in GetFiles())
            {
                if (file.Name == fileName) return true;
            }

            if (recursive)
            {
                foreach (MemFSDir dir in GetDirs())
                {
                    if (dir.ContainsFile(fileName, true)) return true;
                }
            }

            return false;
        }

        public override Dir422 CreateDir(string dirName)
        {
            if (ContainsPathChars(dirName) || String.IsNullOrEmpty(dirName)) return null;
            if (ContainsDir(dirName, false)) return GetDir(dirName);

            MemFSDir newDir = new MemFSDir(dirName, this);
            Dirs.Add(newDir);
            return newDir;
        }


        public override File422 CreateFile(string fileName)
        {
            if (ContainsPathChars(fileName) || String.IsNullOrEmpty(fileName)) return null;

            MemFSFile newFile = new MemFSFile(fileName, this);
            Files.Add(newFile);
            return newFile;
        }

        public override Dir422 GetDir(string dirName)
        {
            if (ContainsPathChars(dirName)) return null;

            foreach (var dir in GetDirs())
            {
                if (dir.Name == dirName) return dir;
            }

            return null;
        }





        public override IList<Dir422> GetDirs()
        {
            return Dirs;
        }

        public override File422 GetFile(string fileName)
        {
            if (ContainsPathChars(fileName)) return null;

            foreach (var file in GetFiles())
            {
                if (file.Name == fileName) return file;
            }

            return null;
        }


        public override IList<File422> GetFiles()
        {
            return Files;
        }
    }

    public class MemFSFile : File422
    {
        private Object thisLock = new Object();
        private Stream MyStream { get; set; }

        public MemFSFile(string fileName, MemFSDir parent)
        {
            Parent = parent;
            Name = fileName;
            var memStream = new MemoryStream();
            MyStream = Stream.Synchronized(memStream);
        }

        public override string Name
        {
            get;
        }

        public override Dir422 Parent
        {
            get;
        }

        public override Stream OpenReadOnly()
        {
            lock (thisLock)
            {
                return MyStream;  
            }
        }

        public override Stream OpenReadWrite()
        {
            lock (thisLock)
            {
                return MyStream;
            }
        }
    }


}
