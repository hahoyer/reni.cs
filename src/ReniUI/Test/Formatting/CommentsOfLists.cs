using hw.UnitTest;

namespace ReniUI.Test.Formatting
{
    [UnitTest]
    [Lists]
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
    }
}