using HWClassLibrary.Debug;
using HWClassLibrary.Helper;

namespace HWFileSystem
{
    internal abstract class File : Dumpable
    {
        private readonly Config _config;

        public File(Config config)
        {
            _config = config;
        }

        [DumpData(false)]
        internal Config Config { get { return _config; } }


        internal protected static Set<Source> CreateMSFileSystemSources(string rootDirName, string name)
        {
            Set<Source> sources = new Set<Source>();
            sources.Add(new MSFileSystemSource(rootDirName + "\\" + name));
            return sources;
        }

        internal virtual void Add(File otherFile)
        {
            Config.Add(otherFile.Config);
        }
    }
}