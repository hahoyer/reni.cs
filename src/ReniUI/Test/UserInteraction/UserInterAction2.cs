using hw.UnitTest;

namespace ReniUI.Test.UserInteraction;

[TestFixture]
[UnitTest]
[UserInterAction]
public sealed class UserInterAction2 : DependenceProvider
{
    const string Text = @"#(aa comment aa)# name!mutable : 3";
    const string Type = @"cccccccccccccccccwiiiikkkkkkkkwkwn";

    [Test]
    [UnitTest]
    public void GetTokenForPosition()
    {
        var compiler = CompilerBrowser.FromText(Text);

        var typeCharacters = new string
        (
            Text
                .Length
                .Select(item => compiler.GetToken(item).TypeCharacter)
                .ToArray()
        );
        (Type == typeCharacters).Assert
        (() =>
            "\nXpctd: " + Type + "\nFound: " + typeCharacters + "\nText : " + Text
        );
    }
}