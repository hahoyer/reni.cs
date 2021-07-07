//01:17:11.454 06.07.2021 ran 172 of 180 

namespace hw.UnitTest
{
    public static class PendingTests
    {
        public static void Run()
        {
        
// error 

TestRunner.RunTest(new ReniUI.Test.FormattingMultiLines().ReformatComments);
TestRunner.RunTest(new ReniUI.Test.FormattingSimple().ReformatComments);
TestRunner.RunTest(new ReniUI.Test.ThenElseMatching().Matching);
TestRunner.RunTest(new ReniUI.Test.ThenElseMatching().NestedMatching);
TestRunner.RunTest(new ReniUI.Test.TokenLocating().FromSourcePart);
TestRunner.RunTest(new ReniUI.Test.TokenLocating().CommentFromSourcePart);
TestRunner.RunTest(new ReniUI.Test.TokenLocating().CompoundSourcePart);
TestRunner.RunTest(new ReniUI.Test.TokenLocating().NamedCompoundSourcePart);
TestRunner.RunTest(new ReniUI.Test.UserInterAction().TypingAProgram);
TestRunner.RunTest(new ReniUI.Test.UserInterAction().GetTokenForPosition);

// dependanterror 

TestRunner.RunTest(new ReniUI.Test.FormattingOfBadThings().BadArgDeclaration);
TestRunner.RunTest(new ReniUI.Test.Formatting().HalfList);
TestRunner.RunTest(new ReniUI.Test.Formatting().LabeledList);
TestRunner.RunTest(new ReniUI.Test.Formatting().FlatList2);
TestRunner.RunTest(new ReniUI.Test.Formatting().ListEndsWithListToken);
TestRunner.RunTest(new ReniUI.Test.Formatting().ListSeparatorAtEnd);
TestRunner.RunTest(new ReniUI.Test.Formatting().ListLineBreakTest2);
TestRunner.RunTest(new ReniUI.Test.Formatting().ListLineBreakTest3);
TestRunner.RunTest(new ReniUI.Test.Formatting().ListTest1);
TestRunner.RunTest(new ReniUI.Test.Formatting().ListTest2);
TestRunner.RunTest(new ReniUI.Test.Formatting().ListWithDeclarationLineBreakTest1);
TestRunner.RunTest(new ReniUI.Test.Formatting().ListWithDeclarationLineBreakTest2);
TestRunner.RunTest(new ReniUI.Test.Formatting().ListWithDeclarationLineBreakTest3);
TestRunner.RunTest(new ReniUI.Test.Formatting().ListWithDeclarationLineBreakTestA2);
TestRunner.RunTest(new ReniUI.Test.Formatting().ListWithDeclarationLineBreakTestA3);
TestRunner.RunTest(new ReniUI.Test.Formatting().ListWithDeclarationLineBreakTestA4);
TestRunner.RunTest(new ReniUI.Test.Formatting().ListWithDeclarationLineBreakTestAA4);
TestRunner.RunTest(new ReniUI.Test.Formatting().ListWithDeclarationTest1);
TestRunner.RunTest(new ReniUI.Test.Formatting().ListWithDeclarationTest2);
TestRunner.RunTest(new ReniUI.Test.Formatting().ListWithDeclarationTest3);
TestRunner.RunTest(new ReniUI.Test.Formatting().MultilineBreakTest);
TestRunner.RunTest(new ReniUI.Test.Formatting().MultilineBreakTest1);
TestRunner.RunTest(new ReniUI.Test.Formatting().Reformat);
TestRunner.RunTest(new ReniUI.Test.Formatting().Reformat1_120);
TestRunner.RunTest(new ReniUI.Test.Formatting().Reformat1_120Temp);
TestRunner.RunTest(new ReniUI.Test.Formatting().Reformat2);
TestRunner.RunTest(new ReniUI.Test.Formatting().ReformatWithComments);
TestRunner.RunTest(new ReniUI.Test.Formatting().SimpleLineCommentFromSourcePart);
TestRunner.RunTest(new ReniUI.Test.Formatting().SingleElementList);
TestRunner.RunTest(new ReniUI.Test.Formatting().SingleElementListFlat);
TestRunner.RunTest(new ReniUI.Test.Formatting().StraightList);
TestRunner.RunTest(new ReniUI.Test.Formatting().TwoLevelParenthesis);
TestRunner.RunTest(new ReniUI.Test.Formatting().UseLineBreakBeforeParenthesis);
TestRunner.RunTest(new ReniUI.Test.Formatting().UseSpaceWhenLineBreakIsRemoved);
TestRunner.RunTest(new ReniUI.Test.UserInterAction2().GetTokenForPosition);
TestRunner.RunTest(new ReniUI.Test.UserInterAction3().GetTokenForPosition);

// notrun 

TestRunner.RunTest(new ReniUI.Test.AutoComplete().GetDeclarationOptions);
TestRunner.RunTest(new ReniUI.Test.AutoCompleteFunctionInCompound().GetDeclarationOptions);
TestRunner.RunTest(new ReniUI.Test.AutoCompleteSimple().GetDeclarationOptions);
TestRunner.RunTest(new ReniUI.Test.FormattingLong().ReformatPart);
}}}
