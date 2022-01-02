using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.UnitTest;
using Reni.Validation;
using ReniUI.Test.Formatting;

namespace ReniUI.Test
{
    [TestFixture]
    [UnitTest]
    [Comments]
    public sealed class StrangeExpressions : DependenceProvider
    {
        [Test]
        [UnitTest]
        public void StringPrefix()
        {
            var text = "someOperand 'string' ";
            var compiler = CompilerBrowser.FromText(text);
            var issues = compiler.Issues.ToArray();
            (issues.Length == 1).Assert();
            var issue = issues[0];
            (issue.IssueId == IssueId.InvalidSuffixExpression).Assert();
            (issue.AdditionalMessage == "(actual: terminal)").Assert();

            var x = compiler.Locate(text.IndexOf("som"));
            x.AssertNotNull();
            var y = compiler.Locate(text.IndexOf("'"));
            y.AssertNotNull();
        }
    }
}