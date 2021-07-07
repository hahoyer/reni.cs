//23:20:06.863 07.07.2021 ran 171 of 180 

namespace hw.UnitTest
{
    public static class PendingTests
    {
        public static void Run()
        {
        
// error 

TestRunner.RunTest(new ReniUI.Test.DeclarationFormatting().LabeledList);
TestRunner.RunTest(new ReniUI.Test.DeclarationFormatting().ListWithDeclarationLineBreakTest1);
TestRunner.RunTest(new ReniUI.Test.DeclarationFormatting().ListWithDeclarationLineBreakTest2);
TestRunner.RunTest(new ReniUI.Test.DeclarationFormatting().ListWithDeclarationLineBreakTest3);
TestRunner.RunTest(new ReniUI.Test.DeclarationFormatting().ListWithDeclarationLineBreakTestA2);
TestRunner.RunTest(new ReniUI.Test.DeclarationFormatting().ListWithDeclarationLineBreakTestA3);
TestRunner.RunTest(new ReniUI.Test.DeclarationFormatting().ListWithDeclarationLineBreakTestA4);
TestRunner.RunTest(new ReniUI.Test.DeclarationFormatting().ListWithDeclarationLineBreakTestAA4);
TestRunner.RunTest(new ReniUI.Test.DeclarationFormatting().ListWithDeclarationTest1);
TestRunner.RunTest(new ReniUI.Test.DeclarationFormatting().ListWithDeclarationTest2);
TestRunner.RunTest(new ReniUI.Test.DeclarationFormatting().ListWithDeclarationTest3);
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
TestRunner.RunTest(new ReniUI.Test.Formatting().Reformat);
TestRunner.RunTest(new ReniUI.Test.Formatting().Reformat1_120);
TestRunner.RunTest(new ReniUI.Test.Formatting().Reformat1_120Temp);
TestRunner.RunTest(new ReniUI.Test.Formatting().Reformat2);
TestRunner.RunTest(new ReniUI.Test.Formatting().TwoLevelParenthesis);
TestRunner.RunTest(new ReniUI.Test.Formatting().UseLineBreakBeforeParenthesis);
TestRunner.RunTest(new ReniUI.Test.Formatting().HalfList);
TestRunner.RunTest(new ReniUI.Test.FormattingWithComments().ReformatComments);
TestRunner.RunTest(new ReniUI.Test.UserInterAction2().GetTokenForPosition);
TestRunner.RunTest(new ReniUI.Test.UserInterAction3().GetTokenForPosition);

// notrun 

TestRunner.RunTest(new ReniUI.Test.AutoComplete().GetDeclarationOptions);
TestRunner.RunTest(new ReniUI.Test.AutoCompleteFunctionInCompound().GetDeclarationOptions);
TestRunner.RunTest(new ReniUI.Test.AutoCompleteSimple().GetDeclarationOptions);
TestRunner.RunTest(new ReniUI.Test.FormattingLong().ReformatPart);
}}}
