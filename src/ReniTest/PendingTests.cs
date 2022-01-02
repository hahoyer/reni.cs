//00:53:45.125 02.01.2022 ran 177 of 184 

// ReSharper disable once CheckNamespace
namespace hw.UnitTest
{
    public static class PendingTests
    {
        public static void Run()
        {
        
// error 

TestRunner.RunTest(new ReniUI.Test.Formatting.Lists().SingleElementList);
TestRunner.RunTest(new ReniUI.Test.Formatting.Lists().StraightListWithMultipleBrackets);
TestRunner.RunTest(new ReniUI.Test.Formatting.ThenElse().NestedThenElseWithBracketsAndWithLineBreak);

// dependanterror 

TestRunner.RunTest(new ReniUI.Test.Formatting.Exclamation().Simple);
TestRunner.RunTest(new ReniUI.Test.Formatting.Exclamation().OneSpace);
TestRunner.RunTest(new ReniUI.Test.Formatting.Exclamation().MultipleTags);
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
TestRunner.RunTest(new ReniUI.Test.Formatting.Declaration().NameIsLeftOfColon);
TestRunner.RunTest(new ReniUI.Test.Formatting.Declaration().SymbolIsLeftOfColon);
TestRunner.RunTest(new ReniUI.Test.Formatting.Declaration().ListWithDeclarationLineBreakTest1);
TestRunner.RunTest(new ReniUI.Test.Formatting.Declaration().LabeledList);
TestRunner.RunTest(new ReniUI.Test.Formatting.Declaration().ListWithDeclarationLineBreakTest2);
TestRunner.RunTest(new ReniUI.Test.Formatting.Declaration().ListWithDeclarationLineBreakTest3);
TestRunner.RunTest(new ReniUI.Test.Formatting.Declaration().ListWithDeclarationLineBreakTestA2);
TestRunner.RunTest(new ReniUI.Test.Formatting.Declaration().ListWithDeclarationLineBreakTestA3);
TestRunner.RunTest(new ReniUI.Test.Formatting.Declaration().ListWithDeclarationLineBreakTestA4);
TestRunner.RunTest(new ReniUI.Test.Formatting.Declaration().ListWithDeclarationLineBreakTestAA4);
TestRunner.RunTest(new ReniUI.Test.Formatting.Declaration().ListWithDeclarationTest1);
TestRunner.RunTest(new ReniUI.Test.Formatting.Declaration().ListWithDeclarationTest2);
TestRunner.RunTest(new ReniUI.Test.Formatting.Declaration().ListWithDeclarationTest3);
TestRunner.RunTest(new ReniUI.Test.Formatting.WithComments().ReformatComments);

// notrun 

TestRunner.RunTest(new ReniUI.Test.Formatting.LongTest().ReformatPart);
}}}
