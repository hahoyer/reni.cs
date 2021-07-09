//14:37:45.717 09.07.2021 ran 157 of 181 

namespace hw.UnitTest
{
    public static class PendingTests
    {
        public static void Run()
        {
        
// error 

TestRunner.RunTest(new Reni.FeatureTest.ScopeHandlingGroup().Run);
TestRunner.RunTest(new ReniUI.Test.BadUserInterAction().GetTokenForPosition);
TestRunner.RunTest(new ReniUI.Test.BadUserInterAction().GetTokenForPositionSimple);
TestRunner.RunTest(new ReniUI.Test.ListFormatting().SimpleLine);
TestRunner.RunTest(new ReniUI.Test.ListFormatting().SingleElementList);
TestRunner.RunTest(new ReniUI.Test.ListFormatting().SingleElementListFlat);
TestRunner.RunTest(new ReniUI.Test.ListFormatting().StraightList);
TestRunner.RunTest(new ReniUI.Test.ListFormatting().StraightListWithMultipleBrackets);
TestRunner.RunTest(new ReniUI.Test.ListFormatting().FlatList2);
TestRunner.RunTest(new ReniUI.Test.ListFormatting().ListEndsWithListToken);
TestRunner.RunTest(new ReniUI.Test.ListFormatting().ListSeparatorAtEnd);
TestRunner.RunTest(new ReniUI.Test.ListFormatting().ListLineBreakTest2);
TestRunner.RunTest(new ReniUI.Test.ListFormatting().ListLineBreakTest3);
TestRunner.RunTest(new ReniUI.Test.ListFormatting().ListTest1);
TestRunner.RunTest(new ReniUI.Test.ListFormatting().ListTest2);
TestRunner.RunTest(new ReniUI.Test.ListFormatting().MultilineBreakTest);
TestRunner.RunTest(new ReniUI.Test.ListFormatting().MultilineBreakTest1);
TestRunner.RunTest(new ReniUI.Test.ListFormatting().MultilineBreakTest11);
TestRunner.RunTest(new ReniUI.Test.ThenElseMatching().Matching);
TestRunner.RunTest(new ReniUI.Test.ThenElseMatching().NestedMatching);
TestRunner.RunTest(new ReniUI.Test.TokenLocating().FromSourcePart);
TestRunner.RunTest(new ReniUI.Test.TokenLocating().CommentFromSourcePart);
TestRunner.RunTest(new ReniUI.Test.TokenLocating().CompoundSourcePart);
TestRunner.RunTest(new ReniUI.Test.TokenLocating().NamedCompoundSourcePart);
TestRunner.RunTest(new ReniUI.Test.UserInterAction().TypingAProgram);
TestRunner.RunTest(new ReniUI.Test.UserInterAction().GetTokenForPosition);

// dependanterror 

TestRunner.RunTest(new Reni.FeatureTest.ComplexContext().Run);
TestRunner.RunTest(new Reni.FeatureTest.AllScopeHandling().Run);
TestRunner.RunTest(new Reni.FeatureTest.Text1().Run);
TestRunner.RunTest(new Reni.FeatureTest.TextConcat().Run);
TestRunner.RunTest(new Reni.FeatureTest.UserObjects().Run);
TestRunner.RunTest(new Reni.FeatureTest.MixIn.Function().Run);
TestRunner.RunTest(new Reni.FeatureTest.MixIn.Simple().Run);
TestRunner.RunTest(new Reni.FeatureTest.Reference.ArrayReferenceAll().Run);
TestRunner.RunTest(new Reni.FeatureTest.Reference.ArrayReferenceByInstance().Run);
TestRunner.RunTest(new Reni.FeatureTest.Reference.ArrayReferenceCopy().Run);
TestRunner.RunTest(new Reni.FeatureTest.Reference.ArrayReferenceCopyAssign().Run);
TestRunner.RunTest(new Reni.FeatureTest.Reference.ArrayReferenceDumpLoop().Run);
TestRunner.RunTest(new Reni.FeatureTest.Reference.FunctionalDumpSimple().Run);
TestRunner.RunTest(new Reni.FeatureTest.Reference.ArrayReferenceDumpSimple().Run);
TestRunner.RunTest(new ReniUI.Test.DeclarationFormatting().ListWithDeclarationLineBreakTest1);
TestRunner.RunTest(new ReniUI.Test.DeclarationFormatting().LabeledList);
TestRunner.RunTest(new ReniUI.Test.DeclarationFormatting().ListWithDeclarationLineBreakTest2);
TestRunner.RunTest(new ReniUI.Test.DeclarationFormatting().ListWithDeclarationLineBreakTest3);
TestRunner.RunTest(new ReniUI.Test.DeclarationFormatting().ListWithDeclarationLineBreakTestA2);
TestRunner.RunTest(new ReniUI.Test.DeclarationFormatting().ListWithDeclarationLineBreakTestA3);
TestRunner.RunTest(new ReniUI.Test.DeclarationFormatting().ListWithDeclarationLineBreakTestA4);
TestRunner.RunTest(new ReniUI.Test.DeclarationFormatting().ListWithDeclarationLineBreakTestAA4);
TestRunner.RunTest(new ReniUI.Test.DeclarationFormatting().ListWithDeclarationTest1);
TestRunner.RunTest(new ReniUI.Test.DeclarationFormatting().ListWithDeclarationTest2);
TestRunner.RunTest(new ReniUI.Test.DeclarationFormatting().ListWithDeclarationTest3);
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
