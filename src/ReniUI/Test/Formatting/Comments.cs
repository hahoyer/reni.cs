using hw.UnitTest;
using NUnit.Framework;

namespace ReniUI.Test.Formatting
{
    [UnitTest]
    [TestFixture]
    public class Comments : DependenceProvider
    {
        [UnitTest]
        [Test]
        public void MultiLineCommentSingleLine()
        {
            const string text =
                @"#( asdf )#  texxxxxxxxxxxt
";

            const string expectedText =
                @"#( asdf )# texxxxxxxxxxxt
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