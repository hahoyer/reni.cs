//13:29:13.309 10.10.2020 run 86 of 178 Reni.FeatureTest.Structure.AccessMember.Run

namespace hw.UnitTest
{
    public static class PendingTests
    {
        public static void Run()
        {
        
// active 

TestRunner.RunTest(new Reni.FeatureTest.Structure.AccessMember().Run);

// error 

TestRunner.RunTest(new ReniUI.Test.AutoCompleteSimple().GetDeclarationOptions);
TestRunner.RunTest(new ReniUI.Test.BadUserInterAction().GetTokenForPosition);
TestRunner.RunTest(new ReniUI.Test.BraceMatching().MatchingBraces);
TestRunner.RunTest(new ReniUI.Test.BraceMatching().MoreMatchingBraces);
TestRunner.RunTest(new ReniUI.Test.BraceMatching().NotMatchingBraces);
TestRunner.RunTest(new ReniUI.Test.ListMatching().Matching);
TestRunner.RunTest(new ReniUI.Test.ListMatching().CombinationsOfMatching);
TestRunner.RunTest(new ReniUI.Test.ListMatching().MixedMatching);
TestRunner.RunTest(new ReniUI.Test.ThenElseMatching().Matching);
TestRunner.RunTest(new ReniUI.Test.ThenElseMatching().NestedMatching);
TestRunner.RunTest(new ReniUI.Test.TokenLocating().FromSourcePart);
TestRunner.RunTest(new ReniUI.Test.TokenLocating().CommentFromSourcePart);
TestRunner.RunTest(new ReniUI.Test.TokenLocating().CompoundSourcePart);
TestRunner.RunTest(new ReniUI.Test.TokenLocating().NamedCompoundSourcePart);
TestRunner.RunTest(new ReniUI.Test.UserInterAction().TypingAProgram);
TestRunner.RunTest(new ReniUI.Test.UserInterAction().GetTokenForPosition);

// dependanterror 

TestRunner.RunTest(new ReniUI.Test.AutoCompleteFunctionInCompound().GetDeclarationOptions);
TestRunner.RunTest(new ReniUI.Test.UserInterAction2().GetTokenForPosition);
TestRunner.RunTest(new ReniUI.Test.UserInterAction3().GetTokenForPosition);

// notrun 

TestRunner.RunTest(new Reni.FeatureTest.ComplexContext().Run);
TestRunner.RunTest(new Reni.FeatureTest.UserObjects().Run);
TestRunner.RunTest(new Reni.FeatureTest.Text1().Run);
TestRunner.RunTest(new Reni.FeatureTest.TextConcat().Run);
TestRunner.RunTest(new Reni.FeatureTest.ThenElse.UseThen().Run);
TestRunner.RunTest(new Reni.FeatureTest.ThenElse.UseElse().Run);
TestRunner.RunTest(new Reni.FeatureTest.ThenElse.AutomaticDereferencing().Run);
TestRunner.RunTest(new Reni.FeatureTest.Integer.IntegerPlusInteger().Run);
TestRunner.RunTest(new Reni.FeatureTest.Integer.IntegerPlusNumber().Run);
TestRunner.RunTest(new Reni.FeatureTest.Integer.Clone().Run);
TestRunner.RunTest(new Reni.FeatureTest.Integer.Create().Run);
TestRunner.RunTest(new Reni.FeatureTest.Integer.Integer1().Run);
TestRunner.RunTest(new Reni.FeatureTest.Integer.Integer2().Run);
TestRunner.RunTest(new Reni.FeatureTest.Integer.Integer127().Run);
TestRunner.RunTest(new Reni.FeatureTest.Text.ConvertHexadecimal().Run);
TestRunner.RunTest(new Reni.FeatureTest.BitArrayOp.BitArrayOp().NegativeNumber);
TestRunner.RunTest(new Reni.FeatureTest.BitArrayOp.BitArrayOp().Run);
TestRunner.RunTest(new Reni.FeatureTest.BitArrayOp.Negate().Run);
TestRunner.RunTest(new Reni.FeatureTest.BitArrayOp.TwoPositiveNumbers().Run);
TestRunner.RunTest(new Reni.FeatureTest.BitArrayOp.PositiveNumbers().Run);
TestRunner.RunTest(new Reni.FeatureTest.BitArrayOp.TwoNegativeNumbers().Run);
TestRunner.RunTest(new Reni.FeatureTest.BitArrayOp.NegativeNumbers().Run);
TestRunner.RunTest(new Reni.FeatureTest.TypeType.ApplyTypeOperator().Run);
TestRunner.RunTest(new Reni.FeatureTest.TypeType.ApplyTypeOperatorWithCut().Run);
TestRunner.RunTest(new Reni.FeatureTest.TypeType.TypeOperatorWithVariable().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.Access().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.Access().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.AccessAndAdd().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.AccessAndAddComplex().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.Assignments().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.AssignmentWithCut().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.NamedSimpleAssignment().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.PropertyVariable().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.PropertyVariable().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.SomeVariables().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.StrangeStructs().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.ContextOperatorPrint().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.ContextOperatorAccess().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.ContextOperatorFunctionAccess().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.ContextOperatorPropertyAccess().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.ContextOperatorAssign().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.ContextOperator().Run);
TestRunner.RunTest(new Reni.FeatureTest.MixIn.Function().Run);
TestRunner.RunTest(new Reni.FeatureTest.MixIn.Simple().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.ConstantFunction().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.Function().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.FunctionOfFunction().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.FunctionArgument().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.FunctionVariable().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.FunctionWithNonLocal().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.FunctionWithRefArg().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.FunctionWithRefArg().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.FunctionAssignment().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.ObjectFunction().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.ObjectFunction1().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.ObjectFunction2().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.ObjectProperty().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.PrimitiveRecursiveFunctionByteWithDump().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.PrimitiveRecursiveFunctionHuge().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.PrimitiveRecursiveFunctionSmall().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.PrimitiveRecursiveFunctionWithDump().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.RecursiveFunction().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.SimpleRepeater().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.Repeater().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.SimpleFunction().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.SimpleFunctionWithNonLocal().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.TwoFunctions().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.TwoFunctions1().Run);
TestRunner.RunTest(new Reni.FeatureTest.CleanupSection.Nested().Run);
TestRunner.RunTest(new Reni.FeatureTest.CleanupSection.Simple().Run);
TestRunner.RunTest(new Reni.FeatureTest.CleanupSection.WithReference().Run);
TestRunner.RunTest(new Reni.FeatureTest.BlogExamples.Post120617().Run);
TestRunner.RunTest(new Reni.FeatureTest.BlogExamples.NestedCompound().Run);
TestRunner.RunTest(new Reni.FeatureTest.BlogExamples.WikiExamples().Run);
TestRunner.RunTest(new Reni.FeatureTest.Reference.ArrayElementType().Run);
TestRunner.RunTest(new Reni.FeatureTest.Reference.ArrayElementType1().Run);
TestRunner.RunTest(new Reni.FeatureTest.Reference.ArrayReferenceAll().Run);
TestRunner.RunTest(new Reni.FeatureTest.Reference.ArrayReferenceByInstance().Run);
TestRunner.RunTest(new Reni.FeatureTest.Reference.ArrayReferenceCopy().Run);
TestRunner.RunTest(new Reni.FeatureTest.Reference.ArrayReferenceCopyAssign().Run);
TestRunner.RunTest(new Reni.FeatureTest.Reference.ArrayReferenceDumpLoop().Run);
TestRunner.RunTest(new Reni.FeatureTest.Reference.FunctionalDumpSimple().Run);
TestRunner.RunTest(new Reni.FeatureTest.Reference.ArrayReferenceDumpSimple().Run);
TestRunner.RunTest(new Reni.FeatureTest.Array.TypeOfArrayFromPieces().Run);
TestRunner.RunTest(new Reni.FeatureTest.Array.TypeOfCombineArraysFromPieces().Run);
TestRunner.RunTest(new Reni.FeatureTest.Array.ElementAccessVariableSetterSimple().Run);
TestRunner.RunTest(new Reni.FeatureTest.Array.ElementAccessVariableSetter().Run);
TestRunner.RunTest(new Reni.FeatureTest.Array.TypeOfSimpleArrayFromPiece().Run);
TestRunner.RunTest(new Reni.FeatureTest.Array.TypeOfElementOfSimpleArrayFromPiece().Run);
TestRunner.RunTest(new ReniUI.Test.AutoComplete().GetDeclarationOptions);
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
TestRunner.RunTest(new ReniUI.Test.FormattingLong().ReformatPart);
}}}
