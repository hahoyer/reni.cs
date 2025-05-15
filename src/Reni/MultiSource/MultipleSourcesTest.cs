using hw.UnitTest;
using Reni.Context;
using Reni.FeatureTest.Helper;

namespace Reni.MultiSource;

[UnitTest]
public sealed class MultipleSourcesTest : CompilerTest
{
    [UnitTest]
    public void FilesSimple()
    {
        Parameters.ProcessErrors = true;
        var names = new[] { "System", "Text" }
            .Select(name => (SmbFile.SourceFolder! / name).FullName)
            .ToArray();
        GetFilesAndRunCompiler(names);
    }
}