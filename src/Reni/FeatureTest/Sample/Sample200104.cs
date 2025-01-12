using hw.UnitTest;
using Reni.FeatureTest.Helper;
using Reni.Validation;

namespace Reni.FeatureTest.Sample;

[UnitTest]
[Target(@"!")]
public sealed class Sample200104 : CompilerTest
{
    public Sample200104()
        => Parameters.CompilationLevel = CompilationLevel.Parser;

    protected override void Verify(IEnumerable<Issue> issues)
    {
        var issueArray = issues.ToArray();
        var i = 0;
        (i == issueArray.Length).Assert();
    }
}