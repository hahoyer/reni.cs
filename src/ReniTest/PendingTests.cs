//15:24:00.194 31.12.2022 run 104 of 189 ReniUI.Test.UserInteraction.UserInterAction.TypingAProgram

// ReSharper disable once CheckNamespace
namespace hw.UnitTest;
public static class PendingTests
{
    public static void Run()
    {
    
// active 

TestRunner.RunTest(new ReniUI.Test.UserInteraction.UserInterAction().TypingAProgram);
TestRunner.RunTest(new ReniUI.Test.UserInteraction.UserInterAction().GetTokenForPosition);

// error 

TestRunner.RunTest(new Reni.FeatureTest.Validation.UseOfUndefinedContextSymbol().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.ContextOperatorPrint().Run);
TestRunner.RunTest(new Reni.FeatureTest.CleanupSection.WithReference().Run);
TestRunner.RunTest(new Reni.FeatureTest.BlogExamples.Post120617().Run);
TestRunner.RunTest(new Reni.FeatureTest.Reference.FunctionalDumpSimple().Run);
TestRunner.RunTest(new Reni.FeatureTest.Array.FromTypeAndFunction().Run);
TestRunner.RunTest(new ReniUI.Test.Examples.Enum().Start);

// dependanterror 

TestRunner.RunTest(new Reni.FeatureTest.Validation.SyntaxErrorComment().Run);
TestRunner.RunTest(new Reni.FeatureTest.Validation.IndirectUseOfUndefinedContextSymbol().Run);
TestRunner.RunTest(new Reni.FeatureTest.Reference.ArrayReferenceDumpSimple().Run);

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
TestRunner.RunTest(new Reni.FeatureTest.Reference.ArrayElementType().Run);
TestRunner.RunTest(new Reni.FeatureTest.Reference.ArrayElementType1().Run);
TestRunner.RunTest(new Reni.FeatureTest.Reference.ArrayReferenceAll().Run);
TestRunner.RunTest(new Reni.FeatureTest.Reference.ArrayReferenceByInstance().Run);
TestRunner.RunTest(new Reni.FeatureTest.Reference.ArrayReferenceCopy().Run);
TestRunner.RunTest(new Reni.FeatureTest.Reference.ArrayReferenceCopyAssign().Run);
TestRunner.RunTest(new Reni.FeatureTest.Reference.ArrayReferenceDumpLoop().Run);
TestRunner.RunTest(new Reni.FeatureTest.Array.TypeOfArrayFromPieces().Run);
TestRunner.RunTest(new ReniUI.Test.UserInteraction.BadUserInterAction().GetTokenForPosition);
TestRunner.RunTest(new ReniUI.Test.UserInteraction.BadUserInterAction().GetTokenForPositionSimple);
TestRunner.RunTest(new ReniUI.Test.UserInteraction.BadUserInterAction().DeclarerAtEnd);
TestRunner.RunTest(new ReniUI.Test.UserInteraction.BigExample().Reformat);
TestRunner.RunTest(new ReniUI.Test.UserInteraction.UserInterAction2().GetTokenForPosition);
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
TestRunner.RunTest(new ReniUI.Test.Formatting.LongTest().ReformatPart);
TestRunner.RunTest(new ReniUI.Test.Formatting.WithComments().ReformatComments);
}}
