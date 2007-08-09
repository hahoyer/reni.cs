using HWClassLibrary.Debug;
using HWClassLibrary.Helper;

namespace HWFileSystem
{
    public abstract class File : Dumpable
    {
        private string _name;

        public File(string name)
        {
            _name = name;
        }

        public string Name { get { return _name; } }

        protected static Set<Source> CreateMSFileSystemSources(string rootDirName, string name)
        {
            Set<Source> sources = new Set<Source>();
            sources.Add(new MSFileSystemSource(rootDirName + "\\" + name));
            return sources;
        }
    }
}