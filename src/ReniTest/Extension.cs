using System.Diagnostics;

namespace ReniTest;

static class Extension
{
    internal static string SolutionDir
        => (new StackTrace(true)
                    .GetFrame(0)
                    .AssertNotNull()
                    .GetFileName()
                    .AssertNotNull()
                    .ToSmbFile()
                    .DirectoryName
                + @"\..\..")
            .ToSmbFile()
            .FullName;
}