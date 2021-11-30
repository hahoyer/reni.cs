using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.UnitTest;
using NUnit.Framework;
using Reni.Validation;

namespace ReniUI.Test
{
    [TestFixture]
    [UnitTest]
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
            (issue.Message == "(actual: terminal)").Assert();

            var x = compiler.Locate(text.IndexOf("som"));

            var y = compiler.Locate(text.IndexOf("'"));
            x.AssertNotNull();
        }
    }
}