using hw.UnitTest;
using ReniUI.Test;
using ReniUI.Test.Formatting;

namespace reniUI.Test.Formatting
{
    [UnitTest]
    public sealed class ThenElse : DependenceProvider
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