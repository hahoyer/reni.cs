using hw.UnitTest;
using NUnit.Framework;

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

    }

}