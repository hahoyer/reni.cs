//00:04:08.538 02.01.2022 ran 180 of 184 

// ReSharper disable once CheckNamespace
namespace hw.UnitTest
{
    public static class PendingTests
    {
        public static void Run()
        {
        
// error 

TestRunner.RunTest(new ReniUI.Test.Formatting.Complex().MultilineBreakTest11);
TestRunner.RunTest(new ReniUI.Test.Formatting.Complex().Reformat1_120);
TestRunner.RunTest(new ReniUI.Test.Formatting.Complex().Reformat1_120TopLineBreak);
TestRunner.RunTest(new ReniUI.Test.Formatting.Complex().Reformat1_120EmptyBrackets);
TestRunner.RunTest(new ReniUI.Test.Formatting.Complex().TwoLevelParenthesis);
TestRunner.RunTest(new ReniUI.Test.Formatting.Complex().HalfList);

// dependanterror 

TestRunner.RunTest(new ReniUI.Test.Formatting.BadThings().BadArgDeclaration);
TestRunner.RunTest(new ReniUI.Test.Formatting.WithComments().ReformatComments);

// notrun 

TestRunner.RunTest(new ReniUI.Test.Formatting.LongTest().ReformatPart);
}}}
