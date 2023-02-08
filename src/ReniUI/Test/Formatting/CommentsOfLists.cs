using hw.UnitTest;

namespace ReniUI.Test.Formatting;

[UnitTest]
[Lists]
[LowPriority]
public class CommentsOfLists : DependenceProvider
{
    [UnitTest]
    public void ComplexText1()
    {
        const string text =
            @"#( asdf )# head
#( asdf)# comment )# waggon
";
        const string expectedText =
            @"#( asdf )# head
    #( asdf)# comment )# waggon
";
        text.SimpleFormattingTest(expectedText);
    }

    [UnitTest]
    [Test]
    public void ComplexText()
    {
        const string text =
            @"#( asdf )#  texxxxxxxxxxxt

texxxxxxx

#(
    comment
)#
texxxxxxx

#( asdf)# comment )#  texxxxxxxxxxxt

#(asdf comment )# comment asd)# comment asdf)# asdf)# texxxxxxxxxxxt

#(asdf comment #( comment )# comment asd)# comment asdf)# texxxxxxxxxxxt

#( )# texxxxxxxxxxxt

#()# texxxxxxxxxxxt";


        const string expectedText =
            @"#( asdf )# texxxxxxxxxxxt

    texxxxxxx

#(
    comment
)#
    texxxxxxx

#( asdf)# comment )#  texxxxxxxxxxxt

#(asdf comment )# comment asd)# comment asdf)# asdf)# texxxxxxxxxxxt

#(asdf comment #( comment )# comment asd)# comment asdf)# texxxxxxxxxxxt

#( )# texxxxxxxxxxxt

#()# texxxxxxxxxxxt";


        text.SimpleFormattingTest(expectedText);
    }

    [UnitTest]
    public void LineCommentAfterListElement()
    {
        // ReSharper disable once StringLiteralTypo
        const string text = @"Auswahl: @{Ja: (Type: ^^, Value: ""Ja""), #
Nein: (Type: ^^, Value: ""Nein""),Vielleicht: (Type: ^^, Value: ""Vielleicht"")}";
        const string expected = @"Auswahl: @
{
  Ja: (Type: ^^, Value: ""Ja""), #
  Nein: (Type: ^^, Value: ""Nein""),
  Vielleicht: (Type: ^^, Value: ""Vielleicht"")
}";
        text.SimpleFormattingTest(expected, indentCount: 2);
    }
}