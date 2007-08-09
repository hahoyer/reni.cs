using HWClassLibrary.Debug;
using HWClassLibrary.Helper;

namespace HWFileSystem
{
    public abstract class Source : Dumpable
    {
        public virtual Set<File> Files
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }
    }
}