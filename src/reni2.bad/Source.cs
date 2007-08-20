using System.Collections.Generic;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;

namespace HWFileSystem
{
    internal abstract class Source : Dumpable
    {
        internal virtual Files Files(NoCaseStringDictionary<Config> config)
        {
            return new Files();
        }
    }
}