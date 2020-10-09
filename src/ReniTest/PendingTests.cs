//23:47:20.065 08.10.2020 ran 92 of 178 

namespace hw.UnitTest
{
    public static class PendingTests
    {
        public static void Run()
        {
        
// error 

TestRunner.RunTest(new Reni.FeatureTest.Structure.AccessSimple1().Run);
TestRunner.RunTest(new Reni.FeatureTest.CleanupSection.Basic().Run);
TestRunner.RunTest(new Reni.FeatureTest.Reference.FunctionalDumpSimple().Run);
TestRunner.RunTest(new Reni.FeatureTest.Array.ElementAccessSimple().Run);
TestRunner.RunTest(new Reni.FeatureTest.Array.FromTypeAndFunction().Run);
TestRunner.RunTest(new Reni.FeatureTest.Array.TypeOfElementOfSimpleArrayFromPiece().Run);
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

TestRunner.RunTest(new Reni.FeatureTest.ComplexContext().Run);
TestRunner.RunTest(new Reni.FeatureTest.UserObjects().Run);
TestRunner.RunTest(new Reni.FeatureTest.ThenElse.UseThen().Run);
TestRunner.RunTest(new Reni.FeatureTest.ThenElse.UseElse().Run);
TestRunner.RunTest(new Reni.FeatureTest.ThenElse.AutomaticDereferencing().Run);
TestRunner.RunTest(new Reni.FeatureTest.Text.Hallo01234().Run);
TestRunner.RunTest(new Reni.FeatureTest.TypeType.TypeOperatorWithVariable().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.Access().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.Access().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.AccessAndAdd().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.AccessAndAddComplex().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.Assignments().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.AssignmentWithCut().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.InnerAccess().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.NamedSimpleAssignment().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.SimpleAssignment().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.SimpleAssignment1().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.SomeVariables().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.StrangeStructs().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.AccessMember().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.ContextOperatorPrint().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.ContextOperatorAccess().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.ContextOperatorFunctionAccess().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.ContextOperatorPropertyAccess().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.ContextOperatorAssign().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.ContextOperator().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.ConstantFunction().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.Function().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.FunctionVariable().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.FunctionWithRefArg().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.FunctionWithRefArg().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.SimpleFunction().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.SimpleFunctionWithNonLocal().Run);
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
TestRunner.RunTest(new Reni.FeatureTest.Reference.ArrayReferenceDumpSimple().Run);
TestRunner.RunTest(new Reni.FeatureTest.Array.ElementAccess().Run);
TestRunner.RunTest(new Reni.FeatureTest.Array.ElementAccessVariableSetterSimple().Run);
TestRunner.RunTest(new Reni.FeatureTest.Array.ElementAccessVariableSetter().Run);
TestRunner.RunTest(new Reni.FeatureTest.Array.ElementAccessVariable().Run);
TestRunner.RunTest(new ReniUI.Test.AutoComplete().GetDeclarationOptions);
TestRunner.RunTest(new ReniUI.Test.AutoCompleteFunctionInCompound().GetDeclarationOptions);
TestRunner.RunTest(new ReniUI.Test.UserInterAction2().GetTokenForPosition);
TestRunner.RunTest(new ReniUI.Test.UserInterAction3().GetTokenForPosition);

// notrun 

TestRunner.RunTest(new Reni.ParserTest.AlternativePrioTableTest().Converter);
TestRunner.RunTest(new Reni.ParserTest.AlternativePrioTableTest().ConverterAndProperty);
TestRunner.RunTest(new Reni.ParserTest.AlternativePrioTableTest().KeyWordAsUserObject);
TestRunner.RunTest(new Reni.ParserTest.AlternativePrioTableTest().Run);
TestRunner.RunTest(new Reni.ParserTest.StrangeDeclarationSyntax().Run);
TestRunner.RunTest(new Reni.FeatureTest.Text1().Run);
TestRunner.RunTest(new Reni.FeatureTest.TextConcat().Run);
TestRunner.RunTest(new Reni.FeatureTest.Integer.IntegerPlusInteger().Run);
TestRunner.RunTest(new Reni.FeatureTest.Integer.IntegerPlusNumber().Run);
TestRunner.RunTest(new Reni.FeatureTest.Integer.Clone().Run);
TestRunner.RunTest(new Reni.FeatureTest.Integer.Create().Run);
TestRunner.RunTest(new Reni.FeatureTest.Integer.Integer1().Run);
TestRunner.RunTest(new Reni.FeatureTest.Integer.Integer2().Run);
TestRunner.RunTest(new Reni.FeatureTest.Integer.Integer127().Run);
TestRunner.RunTest(new Reni.FeatureTest.Sample.Sample200104().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.PropertyVariable().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.PropertyVariable().Run);
TestRunner.RunTest(new Reni.FeatureTest.MixIn.Function().Run);
TestRunner.RunTest(new Reni.FeatureTest.MixIn.Simple().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.FunctionOfFunction().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.FunctionArgument().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.FunctionWithNonLocal().Run);
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
TestRunner.RunTest(new Reni.FeatureTest.Function.TwoFunctions().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.TwoFunctions1().Run);
TestRunner.RunTest(new ReniUI.Test.FormattingLong().ReformatPart);
}}}
