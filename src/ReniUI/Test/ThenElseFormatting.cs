using hw.UnitTest;

namespace ReniUI.Test
{
    [UnitTest]
    public sealed class ThenElseFormatting : DependenceProvider
    {
        [UnitTest]
        public void SimpleThen()
        {
            const string text = @"1 then aaa";
            text.SimpleFormattingTest();
        }

        [UnitTest]
        public void SimpleThenElse()
        {
            const string text = @"1 then aaa else bbb";
            text.SimpleFormattingTest();
        }

        [UnitTest]
        public void SimpleThenElse1()
        {
            const string text = @"ccccccc then aaaaaa else bbbbbbb";
            text.SimpleFormattingTest(@"ccccccc
then aaaaaa
else bbbbbbb", 10);
        }
    }
}