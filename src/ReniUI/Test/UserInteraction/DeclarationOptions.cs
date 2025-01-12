using hw.UnitTest;

namespace ReniUI.Test.UserInteraction;

public sealed class DeclarationOptions : DependenceProvider
{
    [UnitTest]
    public void MutilatedFunctionCall()
    {
        const string text = @"Auswahl: @
    {
        Ja: (Type: ^^, Value: ""Ja""),
        Nein: (Type: ^^, Value: ""Nein""),
        Vielleicht: (Type: ^^, Value: ""Vielleicht"")
    }

    x: Auswahl Ja;
    x = Auswahl Nein then ""No match"" du
";
        var compiler = CompilerBrowser.FromText(text);
        var t = compiler.Compiler.BinaryTree.Left!.RightMost.Token.End;
        var declarations = compiler.GetDeclarations(t.Span(0));

        var syntax = compiler.Syntax;
        syntax.AssertIsNotNull();
        false.Assert();
    }
}