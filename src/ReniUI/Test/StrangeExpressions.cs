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
            (issue.Message == "Using terminal as suffix is invalid.").Assert();

            var x = compiler.GetToken(text.IndexOf("som"));
            x.AssertNotNull();
            var y = compiler.GetToken(text.IndexOf("'"));
            y.AssertNotNull();
        }
    }
}