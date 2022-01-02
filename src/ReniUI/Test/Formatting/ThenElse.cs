using hw.UnitTest;

namespace ReniUI.Test.Formatting;

[UnitTest]
[TestFixture]
public sealed class ThenElse : DependenceProvider
{
    [UnitTest]
    [Test]
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
    [Test]
    public void ThenElseWithLineBreak()
    {
        const string text = @"ccccccc then aaaaaa else bbbbbbb";
        text.SimpleFormattingTest(@"ccccccc
then aaaaaa
else bbbbbbb", 10);
    }

    [UnitTest]
    [Test]
    public void ThenWithLineBreak()
    {
        const string text = @"ccccccc then aaaaaa ";
        text.SimpleFormattingTest(@"ccccccc
then aaaaaa", 10);
    }
    [UnitTest]
    [Test]
    public void NestedThenElseWithLineBreak()
    {
        const string text = @"cond1 then cond2 then then2 else cond3 else else3 ";
        // It is not as expected:
        text.SimpleFormattingTest(@"cond1
then
    cond2
    then then2
else
    cond3
    else else3", 7);
        // since it is :
        // (cond1 then (cond2 then then2)) else (cond3 else else3)
        // so better use some brackets:
        // cond1 then (cond2 then then2 else else2) else else1
    }

    [UnitTest]
    [Test]
    public void NestedThenElseWithBracketsAndWithLineBreak()
    {
        const string text = @"cond1 then (cond2 then then2 else else2) else else1";
        text.SimpleFormattingTest(@"cond1
then
(
    cond2
    then then2
    else else2
)
else else1", 7);
    }

}