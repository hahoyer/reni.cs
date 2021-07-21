//22:08:50.489 21.07.2021 ran 172 of 181 

namespace hw.UnitTest
{
    public static class PendingTests
    {
        public static void Run()
        {
        
// error 

TestRunner.RunTest(new ReniUI.Test.Formatting().Reformat);
TestRunner.RunTest(new ReniUI.Test.Formatting().Reformat1_120);
TestRunner.RunTest(new ReniUI.Test.Formatting().Reformat1_120TopLineBreak);
TestRunner.RunTest(new ReniUI.Test.Formatting().Reformat1_120EmptyBrackets);
TestRunner.RunTest(new ReniUI.Test.Formatting().Reformat2);
TestRunner.RunTest(new ReniUI.Test.Formatting().TwoLevelParenthesis);
TestRunner.RunTest(new ReniUI.Test.Formatting().UseLineBreakBeforeParenthesis);
TestRunner.RunTest(new ReniUI.Test.Formatting().HalfList);
TestRunner.RunTest(new ReniUI.Test.ThenElseMatching().Matching);
TestRunner.RunTest(new ReniUI.Test.ThenElseMatching().NestedMatching);
TestRunner.RunTest(new ReniUI.Test.TokenLocating().FromSourcePart);
TestRunner.RunTest(new ReniUI.Test.TokenLocating().CommentFromSourcePart);
TestRunner.RunTest(new ReniUI.Test.TokenLocating().CompoundSourcePart);
TestRunner.RunTest(new ReniUI.Test.TokenLocating().NamedCompoundSourcePart);
TestRunner.RunTest(new ReniUI.Test.UserInterAction().TypingAProgram);
TestRunner.RunTest(new ReniUI.Test.UserInterAction().GetTokenForPosition);

// dependanterror 

TestRunner.RunTest(new ReniUI.Test.BadUserInterAction().GetTokenForPosition);
TestRunner.RunTest(new ReniUI.Test.BadUserInterAction().GetTokenForPositionSimple);
TestRunner.RunTest(new ReniUI.Test.FormattingOfBadThings().BadArgDeclaration);
TestRunner.RunTest(new ReniUI.Test.FormattingWithComments().ReformatComments);
TestRunner.RunTest(new ReniUI.Test.UserInterAction2().GetTokenForPosition);
TestRunner.RunTest(new ReniUI.Test.UserInterAction3().GetTokenForPosition);

// notrun 

TestRunner.RunTest(new ReniUI.Test.AutoComplete().GetDeclarationOptions);
TestRunner.RunTest(new ReniUI.Test.AutoCompleteFunctionInCompound().GetDeclarationOptions);
TestRunner.RunTest(new ReniUI.Test.AutoCompleteSimple().GetDeclarationOptions);
TestRunner.RunTest(new ReniUI.Test.FormattingLong().ReformatPart);
}}}
