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

    }
}
