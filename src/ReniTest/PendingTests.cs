//09:41:42.328 23.10.2020 ran 153 of 178 

namespace hw.UnitTest
{
    public static class PendingTests
    {
        public static void Run()
        {
        
// error 

TestRunner.RunTest(new Reni.FeatureTest.ScopeHandlingGroup().Run);
TestRunner.RunTest(new Reni.FeatureTest.ScopeHandlingMultiple().Run);
TestRunner.RunTest(new Reni.FeatureTest.Validation.UnMatchedLeftParenthesis().Run);
TestRunner.RunTest(new Reni.FeatureTest.Validation.UnMatchedRightParenthesis().Run);
TestRunner.RunTest(new ReniUI.Test.BadUserInterAction().GetTokenForPosition);
TestRunner.RunTest(new ReniUI.Test.BadUserInterAction().GetTokenForPositionSimple);
TestRunner.RunTest(new ReniUI.Test.BracketMatching().MatchingBraces);
TestRunner.RunTest(new ReniUI.Test.BracketMatching().MoreMatchingBraces);
TestRunner.RunTest(new ReniUI.Test.BracketMatching().NotMatchingBraces);
TestRunner.RunTest(new ReniUI.Test.ExpressionFormatting().FromSourcePart);
TestRunner.RunTest(new ReniUI.Test.ExpressionFormatting().BadArgDeclaration);
TestRunner.RunTest(new ReniUI.Test.ListMatching().Matching);
TestRunner.RunTest(new ReniUI.Test.ListMatching().CombinationsOfMatching);
TestRunner.RunTest(new ReniUI.Test.ListMatching().MixedMatching);
TestRunner.RunTest(new ReniUI.Test.StructFormatting().One);
TestRunner.RunTest(new ReniUI.Test.StructFormatting().Two);
TestRunner.RunTest(new ReniUI.Test.StructFormatting().BreakLine);
TestRunner.RunTest(new ReniUI.Test.StructFormatting().BreakLineWithLimit1);
TestRunner.RunTest(new ReniUI.Test.StructFormatting().BreakLineWithLimit0);
TestRunner.RunTest(new ReniUI.Test.StructFormatting().OmitSpaceWhenLineBreakRemains);
TestRunner.RunTest(new ReniUI.Test.StructFormatting().UseSpaceWhenLineBreakIsRemoved);
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
TestRunner.RunTest(new Reni.FeatureTest.UserObjects().Run);
TestRunner.RunTest(new Reni.FeatureTest.Text1().Run);
TestRunner.RunTest(new Reni.FeatureTest.TextConcat().Run);
TestRunner.RunTest(new Reni.FeatureTest.Validation.UnMatchedBrackets().Run);
TestRunner.RunTest(new Reni.FeatureTest.MixIn.Function().Run);
TestRunner.RunTest(new Reni.FeatureTest.MixIn.Simple().Run);
TestRunner.RunTest(new Reni.FeatureTest.Reference.ArrayReferenceAll().Run);
TestRunner.RunTest(new Reni.FeatureTest.Reference.ArrayReferenceByInstance().Run);
TestRunner.RunTest(new Reni.FeatureTest.Reference.ArrayReferenceCopy().Run);
TestRunner.RunTest(new Reni.FeatureTest.Reference.ArrayReferenceCopyAssign().Run);
TestRunner.RunTest(new Reni.FeatureTest.Reference.ArrayReferenceDumpLoop().Run);
TestRunner.RunTest(new Reni.FeatureTest.Reference.FunctionalDumpSimple().Run);
TestRunner.RunTest(new Reni.FeatureTest.Reference.ArrayReferenceDumpSimple().Run);
TestRunner.RunTest(new ReniUI.Test.Formatting().HalfList);
TestRunner.RunTest(new ReniUI.Test.Formatting().LabeledList);
TestRunner.RunTest(new ReniUI.Test.Formatting().LabelsOnToLevel);
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
TestRunner.RunTest(new ReniUI.Test.FormattingMultiLines().ReformatComments);
TestRunner.RunTest(new ReniUI.Test.FormattingSimple().ReformatComments);
TestRunner.RunTest(new ReniUI.Test.StructFormattingCurrent().Start);
TestRunner.RunTest(new ReniUI.Test.StructFormattingCurrent().LabeledEntriesInList);
TestRunner.RunTest(new ReniUI.Test.UserInterAction2().GetTokenForPosition);
TestRunner.RunTest(new ReniUI.Test.UserInterAction3().GetTokenForPosition);

// notrun 

TestRunner.RunTest(new ReniUI.Test.AutoComplete().GetDeclarationOptions);
TestRunner.RunTest(new ReniUI.Test.AutoCompleteFunctionInCompound().GetDeclarationOptions);
TestRunner.RunTest(new ReniUI.Test.AutoCompleteSimple().GetDeclarationOptions);
TestRunner.RunTest(new ReniUI.Test.FormattingLong().ReformatPart);
}}}
