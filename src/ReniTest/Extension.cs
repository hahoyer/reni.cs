using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.Helper;

namespace ReniTest
{
    static class Extension
    {
        internal static string SolutionDir
            => (new StackTrace(true)
                        .GetFrame(0)
                        .GetFileName()
                        .FileHandle()
                        .DirectoryName
                    + @"\..\..")
                .FileHandle()
                .FullName;
    }
}