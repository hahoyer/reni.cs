using hw.UnitTest;

namespace ReniUI.Test.Formatting
{
    [UnitTest]
    [TestFixture]
    [Lists]
    public sealed class Declaration
        : DependenceProvider
    {
        [Test]
        [UnitTest]
        public void NameIsLeftOfColon()
        {
            const string text = @"lllll  :bbbbb";
            const string expectedText = @"lllll: bbbbb";

            text.SimpleFormattingTest(expectedText);
        }

        [Test]
        [UnitTest]
        public void SymbolIsLeftOfColon()
        {
            const string text = @"***  :bbbbb";
            const string expectedText = @"*** : bbbbb";

            text.SimpleFormattingTest(expectedText);
        }

        [Test]
        [UnitTest]
        public void ListWithDeclarationLineBreakTest1()
        {
            const string text = @"lllll  :bbbbb";
            const string expectedText = @"lllll:
    bbbbb";

            text.SimpleFormattingTest(expectedText, 10, 1);
        }

        [Test]
        [UnitTest]
        public void LabeledList()
        {
            const string text = @"llll:(aaaaa;llll:bbbbb;ccccc)";
            const string expectedText = @"llll:
(
    aaaaa;
    llll: bbbbb;
    ccccc
)";

            text.SimpleFormattingTest(expectedText, 12, 1);
        }

        [Test]
        [UnitTest]
        public void ListWithDeclarationLineBreakTest2()
        {
            const string text = @"aaaaa;llll:bbbbb";
            const string expectedText = @"aaaaa;
llll: bbbbb";

            text.SimpleFormattingTest(expectedText, 15, 1);
        }

        [Test]
        [UnitTest]
        public void ListWithDeclarationLineBreakTest3()
        {
            const string text = @"aaaaa;llll:bbbbb;ccccc";
            const string expectedText = @"aaaaa;
llll: bbbbb;
ccccc";

            text.SimpleFormattingTest(expectedText, 20, 1);
        }

        [Test]
        [UnitTest]
        public void ListWithDeclarationLineBreakTestA2()
        {
            const string text = @"aaaaa;llll:bbbbb";
            const string expectedText = @"aaaaa;

llll:
    bbbbb";

            text.SimpleFormattingTest(expectedText, 10, 1);
        }

        [Test]
        [UnitTest]
        public void ListWithDeclarationLineBreakTestA3()
        {
            const string text = @"aaaaa;llll:bbbbb;ccccc";
            const string expectedText = @"aaaaa;

llll:
    bbbbb;

ccccc";

            text.SimpleFormattingTest(expectedText, 10, 1);
        }

        [Test]
        [UnitTest]
        public void ListWithDeclarationLineBreakTestA4()
        {
            const string text = @"aaaaa;aaaaa;llll:bbbbb;ccccc;ddddd";
            const string expectedText = @"aaaaa;
aaaaa;

llll:
    bbbbb;

ccccc;
ddddd";

            text.SimpleFormattingTest(expectedText, 10, 1);
        }

        [Test]
        [UnitTest]
        // ReSharper disable once InconsistentNaming
        public void ListWithDeclarationLineBreakTestAA4()
        {
            const string text = @"aaaaa;aaaaa;llll:bbbbb;mmmmm:22222;ccccc;ddddd";
            const string expectedText = @"aaaaa;
aaaaa;

llll:
    bbbbb;

mmmmm:
    22222;

ccccc;
ddddd";

            text.SimpleFormattingTest(expectedText, 10, 1);
        }

        [Test]
        [UnitTest]
        public void ListWithDeclarationTest1()
        {
            const string text = @"lllll:bbbbb";
            const string expectedText = @"lllll: bbbbb";

            text.SimpleFormattingTest(expectedText, emptyLineLimit: 1);
        }

        [Test]
        [UnitTest]
        public void ListWithDeclarationTest2()
        {
            const string text = @"aaaaa;llll:bbbbb";
            const string expectedText = @"aaaaa; llll: bbbbb";

            text.SimpleFormattingTest(expectedText, emptyLineLimit: 1);
        }

        [Test]
        [UnitTest]
        public void ListWithDeclarationTest3()
        {
            const string text = @"aaaaa;llll:bbbbb;ccccc";
            const string expectedText = @"aaaaa; llll: bbbbb; ccccc";

            text.SimpleFormattingTest(expectedText, emptyLineLimit: 1);
        }
    }
}