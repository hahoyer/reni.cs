using System.Linq;
using hw.DebugFormatter;
using hw.UnitTest;
using Reni.Validation;

namespace ReniUI.Test.Examples;

[UnitTest]
public sealed class Enum : DependenceProvider
{
    [UnitTest]
    public void Start()
    {
        const string text = @"Auswahl: @
{
  Ja: (Type: ^^, Value: ""Ja""),
  Nein: (Type: ^^, Value: ""Nein""),
  Vielleicht: (Type: ^^, Value: ""Vielleicht"")
};

x: Auswahl Ja;
x = Auswahl Nein then ""No match"" dumpprint
";
        var compiler = CompilerBrowser.FromText(text);
        var issues = compiler.Issues.ToArray();

        var i = 0;

        var issue = issues[i++];
        (issue.IssueId == IssueId.MissingDeclarationForType).Assert(issue.Dump);

        issue = issues[i++];
        (issue.IssueId == IssueId.MissingDeclarationForType).Assert(issue.Dump);

        issue = issues[i++];
        (issue.IssueId == IssueId.MissingDeclarationForType).Assert(issue.Dump);

        (i == issues.Length).Assert();

        false.Assert();
    }
}