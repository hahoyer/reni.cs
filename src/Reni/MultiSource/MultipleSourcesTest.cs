using hw.UnitTest;
using Reni.FeatureTest.Helper;
using Reni.FeatureTest.Validation;
using Reni.Validation;

namespace Reni.MultiSource;

[UnitTest]
public sealed class MultipleSourcesTest : CompilerTest
{
    protected override void Verify(IEnumerable<Issue> issues)
    {
        var a = issues.ToArray();
        var i = 0;
        a[i++].IsLogDumpLike(23, 21, 23, 27, IssueId.MissingDeclarationInContext, "\"value1\"").Assert();
        a[i++].IsLogDumpLike(22, 13, 22, 19, IssueId.ConsequenceError, "\"result\"").Assert();
        a[i++].IsLogDumpLike(23, 13, 23, 19, IssueId.ConsequenceError, "\"result\"").Assert();
        (a.Length == i).Assert();
    }

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