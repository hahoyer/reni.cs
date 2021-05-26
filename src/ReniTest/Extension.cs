using System.Diagnostics;
using hw.Helper;

namespace ReniTest
{
    static class Extension
    {
        internal static string SolutionDir
            => (new StackTrace(true)
                        .GetFrame(0)
                        .GetFileName()
                        .ToSmbFile()
                        .DirectoryName +
                    @"\..\..")
                .ToSmbFile()
                .FullName;
    }
}