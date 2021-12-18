//04:15:59.570 18.12.2021 ran 178 of 183 

// ReSharper disable once CheckNamespace
namespace hw.UnitTest
{
    public static class PendingTests
    {
        public static void Run()
        {
        
// error 

TestRunner.RunTest(new ReniUI.Test.Formatting.Basics().BreakLineWithLimit1);
TestRunner.RunTest(new ReniUI.Test.Formatting.Basics().OmitSpaceWhenLineBreakRemains);

// dependanterror 

TestRunner.RunTest(new ReniUI.Test.Formatting.BadThings().BadArgDeclaration);
TestRunner.RunTest(new ReniUI.Test.Formatting.Complex().MultilineBreakTest11);
TestRunner.RunTest(new ReniUI.Test.Formatting.Complex().Reformat);
TestRunner.RunTest(new ReniUI.Test.Formatting.Complex().Reformat1_120);
TestRunner.RunTest(new ReniUI.Test.Formatting.Complex().Reformat1_120TopLineBreak);
TestRunner.RunTest(new ReniUI.Test.Formatting.Complex().Reformat1_120EmptyBrackets);
TestRunner.RunTest(new ReniUI.Test.Formatting.Complex().Reformat2);
TestRunner.RunTest(new ReniUI.Test.Formatting.Complex().TwoLevelParenthesis);
TestRunner.RunTest(new ReniUI.Test.Formatting.Complex().UseLineBreakBeforeParenthesis);
TestRunner.RunTest(new ReniUI.Test.Formatting.Complex().HalfList);
TestRunner.RunTest(new ReniUI.Test.Formatting.WithComments().ReformatComments);

// notrun 

TestRunner.RunTest(new ReniUI.Test.Formatting.LongTest().ReformatPart);
}}}
