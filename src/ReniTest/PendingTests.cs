//13:20:37.672 15.12.2021 ran 176 of 183 

// ReSharper disable once CheckNamespace
namespace hw.UnitTest
{
    public static class PendingTests
    {
        public static void Run()
        {
        
// error 

TestRunner.RunTest(new ReniUI.Test.Formatting.Comments().SingleLine);
TestRunner.RunTest(new ReniUI.Test.Formatting.Comments().SingleLineWithLineBreak);
TestRunner.RunTest(new ReniUI.Test.Formatting.Comments().SingleLineWithVolatileLineBreak);
TestRunner.RunTest(new ReniUI.Test.Formatting.Comments().MultiLineCommentSingleLine);
TestRunner.RunTest(new ReniUI.Test.Formatting.Comments().SeparatorBeforeComment1);
TestRunner.RunTest(new ReniUI.Test.Formatting.Comments().SeparatorBeforeComment2);
TestRunner.RunTest(new ReniUI.Test.Formatting.Comments().SeparatorBeforeComment3);
TestRunner.RunTest(new ReniUI.Test.Formatting.Comments().SeparatorBeforeComment4);
TestRunner.RunTest(new ReniUI.Test.Formatting.Comments().ComplexText);
TestRunner.RunTest(new ReniUI.Test.Formatting.Basics().BreakLine);
TestRunner.RunTest(new ReniUI.Test.Formatting.Basics().One);
TestRunner.RunTest(new ReniUI.Test.Formatting.Basics().Two);
TestRunner.RunTest(new ReniUI.Test.Formatting.Basics().BreakLine3);
TestRunner.RunTest(new ReniUI.Test.Formatting.Basics().BreakLineWithLimit1);
TestRunner.RunTest(new ReniUI.Test.Formatting.Basics().BreakLineWithLimit0);
TestRunner.RunTest(new ReniUI.Test.Formatting.Basics().OmitSpaceWhenLineBreakRemains);
TestRunner.RunTest(new ReniUI.Test.Formatting.Basics().UseSpaceWhenLineBreakIsRemoved);
TestRunner.RunTest(new ReniUI.Test.Formatting.Basics().EmptyBrackets);
TestRunner.RunTest(new ReniUI.Test.Formatting.Basics().Reformat);
TestRunner.RunTest(new ReniUI.Test.Formatting.Lists().SimpleLine);
TestRunner.RunTest(new ReniUI.Test.Formatting.Lists().SingleElementList);
TestRunner.RunTest(new ReniUI.Test.Formatting.Lists().SingleElementListFlat);
TestRunner.RunTest(new ReniUI.Test.Formatting.Lists().StraightList);
TestRunner.RunTest(new ReniUI.Test.Formatting.Lists().StraightListWithMultipleBrackets);
TestRunner.RunTest(new ReniUI.Test.Formatting.Lists().FlatList2);
TestRunner.RunTest(new ReniUI.Test.Formatting.Lists().ListEndsWithListToken);
TestRunner.RunTest(new ReniUI.Test.Formatting.Lists().ListSeparatorAtEnd);
TestRunner.RunTest(new ReniUI.Test.Formatting.Lists().ListLineBreakTest2);
TestRunner.RunTest(new ReniUI.Test.Formatting.Lists().ListLineBreakTest3);
TestRunner.RunTest(new ReniUI.Test.Formatting.Lists().ListTest1);
TestRunner.RunTest(new ReniUI.Test.Formatting.Lists().ListTest2);
TestRunner.RunTest(new ReniUI.Test.Formatting.Lists().MultilineBreakTest);
TestRunner.RunTest(new ReniUI.Test.Formatting.Lists().MultilineBreakTest1);
TestRunner.RunTest(new ReniUI.Test.Formatting.Lists().MultilineBreakTest11);
TestRunner.RunTest(new ReniUI.Test.Formatting.ThenElse().SimpleThen);
TestRunner.RunTest(new ReniUI.Test.Formatting.ThenElse().SimpleThenElse);
TestRunner.RunTest(new ReniUI.Test.Formatting.ThenElse().SimpleThenElse1);

// dependanterror 

TestRunner.RunTest(new ReniUI.Test.StrangeExpressions().StringPrefix);
TestRunner.RunTest(new ReniUI.Test.Formatting.BadThings().BadArgDeclaration);
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
