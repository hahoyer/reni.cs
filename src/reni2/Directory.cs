using HWClassLibrary.Helper;

namespace HWFileSystem
{
    public class Directory : File
    {
        private Set<Source> _soures;

        private Directory(string name, Set<Source> soures)
            : base(name)
        {
            _soures = soures;
        }

        internal static Directory CreateMSFileSystem(string rootDirName, string name)
        {
            return new Directory(name, CreateMSFileSystemSources(rootDirName, name));
        }

        public Set<File> Files
        {
            get
            {
                return Sources.Apply<Set<File>, Set<File>>
                    (
                    delegate(Source x) { return x.Files; },
                    delegate(Set<File> result, Set<File> elementResult) { return result | elementResult; }
                    );
            }
        }

        public Set<Source> Sources { get { return _soures; } }
    }
}