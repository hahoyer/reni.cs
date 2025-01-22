using hw.UnitTest;
using Reni.FeatureTest.Helper;
using Reni.Validation;

namespace ReniUI.Test.Examples;

[UnitTest]
public sealed class EnumDevelop : DependenceProvider
{
    [UnitTest]
    public void Start()
    {
        const string text = """
                            Auswahl: @
                            {
                              Ja: (Type: ^^, Value: "Ja"),
                              Nein: (Type: ^^, Value: "Nein"),
                              Vielleicht: (Type: ^^, Value: "Vielleicht")
                            };

                            x: Auswahl Ja;
                            x = Auswahl Nein then "No match" dumpprint

                            """;
        var compiler = CompilerBrowser.FromText(text);
        var issues = compiler.Issues.ToArray();

        var i = 0;

        var issue = issues[i++];
        (issue.IssueId == IssueId.MissingDeclarationForType).Assert(issue.Dump);

        issue = issues[i++];
        (issue.IssueId == IssueId.ConsequenceError).Assert(issue.Dump);

        issue = issues[i++];
        (issue.IssueId == IssueId.MissingDeclarationForType).Assert(issue.Dump);

        (i == issues.Length).Assert();
    }

    [UnitTest]
    public void DumpPrintCorrected()
    {
        const string text =
            """
            Auswahl: @
            {
              Ja: (Type: ^^, Value: "Ja"),
              Nein: (Type: ^^, Value: "Nein"),
              Vielleicht: (Type: ^^, Value: "Vielleicht")
            };

            x: Auswahl Ja;
            x = Auswahl Nein then "No match" dump_print

            """;
        var compiler = CompilerBrowser.FromText(text);
        var issues = compiler.Issues.ToArray();

        var i = 0;

        var issue = issues[i++];
        (issue.IssueId == IssueId.MissingDeclarationForType).Assert(issue.Dump);

        issue = issues[i++];
        (issue.IssueId == IssueId.ConsequenceError).Assert(issue.Dump);

        (i == issues.Length).Assert();
    }

    [UnitTest]
    public void Corrected()
    {
        const string text =
            """
            Auswahl: @!
              {
            Ja!public : (Type: ^^, Value: "Ja"),
            Nein!public  : (Type: ^^, Value: "Nein"),
            Vielleicht!public  : (Type: ^^, Value: "Vielleicht")
            };

            x: Auswahl Ja;
            x == Auswahl Ja then "Match" dump_print;
            x == Auswahl Nein then "No match" dump_print

            """;
        var compiler = CompilerBrowser.FromText(text);
        var issues = compiler.Issues.ToArray();

        var i = 0;

        (i == issues.Length).Assert();
    }
}

[UnitTest]
[TargetSet(
    """
    Auswahl: @!
      {
    Ja!public : (Type: ^^, Value: "Ja"),
    Nein!public  : (Type: ^^, Value: "Nein"),
    Vielleicht!public  : (Type: ^^, Value: "Vielleicht")
    };

    x: Auswahl Ja;
    x == Auswahl Ja then "Match" dump_print;
    x == Auswahl Nein then "No match" dump_print
    """,
    "MatchNo match")]
public sealed class EnumExample : CompilerTest;