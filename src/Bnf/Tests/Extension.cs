using System.Diagnostics;
using hw.Helper;

namespace Bnf.Tests
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