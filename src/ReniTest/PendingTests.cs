//19:46:15.502 25.07.2021 ran 45 of 183 

namespace hw.UnitTest
{
    public static class PendingTests
    {
        public static void Run()
        {
        
// error 

TestRunner.RunTest(new Reni.ParserTest.AlternativePrioTableTest().Converter);
TestRunner.RunTest(new Reni.ParserTest.AlternativePrioTableTest().ConverterAndProperty);
TestRunner.RunTest(new Reni.ParserTest.AlternativePrioTableTest().KeyWordAsUserObject);
TestRunner.RunTest(new Reni.ParserTest.AlternativePrioTableTest().Run);
TestRunner.RunTest(new Reni.ParserTest.StrangeDeclarationSyntax().Run);
TestRunner.RunTest(new Reni.FeatureTest.ScopeHandlingPublic().Run);
TestRunner.RunTest(new Reni.FeatureTest.ScopeHandlingNonPublic().Run);
TestRunner.RunTest(new Reni.FeatureTest.ScopeHandlingError().Run);
TestRunner.RunTest(new Reni.FeatureTest.Validation.UndefinedContextSymbol().Run);
TestRunner.RunTest(new Reni.FeatureTest.Validation.UndefinedSymbol().Run);
TestRunner.RunTest(new Reni.FeatureTest.Validation.UnMatchedLeftParenthesis().Run);
TestRunner.RunTest(new Reni.FeatureTest.Validation.UnMatchedRightParenthesis().Run);
TestRunner.RunTest(new Reni.FeatureTest.Text.Hallo().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.TwoStatements().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.List1().Run);
TestRunner.RunTest(new Reni.FeatureTest.DefaultOperations.Equal().Run);
TestRunner.RunTest(new Reni.FeatureTest.DefaultOperations.NotEqual().Run);
TestRunner.RunTest(new Reni.FeatureTest.DefaultOperations.GreaterThan().Run);
TestRunner.RunTest(new Reni.FeatureTest.DefaultOperations.LessThan().Run);
TestRunner.RunTest(new Reni.FeatureTest.DefaultOperations.LessOrEqual().Run);
TestRunner.RunTest(new Reni.FeatureTest.DefaultOperations.GreaterOrEqual().Run);
TestRunner.RunTest(new Reni.FeatureTest.CleanupSection.Basic().Run);
TestRunner.RunTest(new Reni.FeatureTest.BlogExamples.Post120109().Run);
TestRunner.RunTest(new Reni.FeatureTest.Reference.ReferenceSimple().Run);
TestRunner.RunTest(new Reni.FeatureTest.Array.DefaultInitialized().Run);
TestRunner.RunTest(new Reni.FeatureTest.Array.SimpleArrayFromPiece().Run);
TestRunner.RunTest(new ReniUI.Test.PairedSyntaxTree().SimpleList);
TestRunner.RunTest(new ReniUI.Test.PairedSyntaxTree().TopLevelList);
TestRunner.RunTest(new ReniUI.Test.PairedSyntaxTree().List);
TestRunner.RunTest(new ReniUI.Test.PairedSyntaxTree().List3);
TestRunner.RunTest(new ReniUI.Test.ThenElseMatching().Matching);
TestRunner.RunTest(new ReniUI.Test.ThenElseMatching().NestedMatching);
TestRunner.RunTest(new ReniUI.Test.TokenLocating().FromSourcePart);
TestRunner.RunTest(new ReniUI.Test.TokenLocating().CommentFromSourcePart);
TestRunner.RunTest(new ReniUI.Test.TokenLocating().CompoundSourcePart);
TestRunner.RunTest(new ReniUI.Test.TokenLocating().NamedCompoundSourcePart);
TestRunner.RunTest(new ReniUI.Test.UserInteraction.UserInterAction().TypingAProgram);
TestRunner.RunTest(new ReniUI.Test.UserInteraction.UserInterAction().GetTokenForPosition);
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

// dependanterror 

TestRunner.RunTest(new Reni.FeatureTest.ComplexContext().Run);
TestRunner.RunTest(new Reni.FeatureTest.ScopeHandlingGroup().Run);
TestRunner.RunTest(new Reni.FeatureTest.ScopeHandlingMultiple().Run);
TestRunner.RunTest(new Reni.FeatureTest.PublicNonPublic1().Run);
TestRunner.RunTest(new Reni.FeatureTest.PublicNonPublic2().Run);
TestRunner.RunTest(new Reni.FeatureTest.AllScopeHandling().Run);
TestRunner.RunTest(new Reni.FeatureTest.UserObjects().Run);
TestRunner.RunTest(new Reni.FeatureTest.Validation.SyntaxErrorComment().Run);
TestRunner.RunTest(new Reni.FeatureTest.Validation.UseOfUndefinedContextSymbol().Run);
TestRunner.RunTest(new Reni.FeatureTest.Validation.IndirectUseOfUndefinedContextSymbol().Run);
TestRunner.RunTest(new Reni.FeatureTest.Validation.UnMatchedBrackets().Run);
TestRunner.RunTest(new Reni.FeatureTest.TypeType.ApplyTypeOperator().Run);
TestRunner.RunTest(new Reni.FeatureTest.TypeType.ApplyTypeOperatorWithCut().Run);
TestRunner.RunTest(new Reni.FeatureTest.TypeType.TypeOperator().Run);
TestRunner.RunTest(new Reni.FeatureTest.Text.Hallo01234().Run);
TestRunner.RunTest(new Reni.FeatureTest.Text.HalloApo().Run);
TestRunner.RunTest(new Reni.FeatureTest.Text.HalloApoApo().Run);
TestRunner.RunTest(new Reni.FeatureTest.Text.HalloApoQuoApo().Run);
TestRunner.RunTest(new Reni.FeatureTest.Text.HalloWelt().Run);
TestRunner.RunTest(new Reni.FeatureTest.Text.ConvertFromNumber().Run);
TestRunner.RunTest(new Reni.FeatureTest.Text.ConvertFromNumbers().Run);
TestRunner.RunTest(new Reni.FeatureTest.Text.ConvertHexadecimal().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.Access().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.Access().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.AccessAndAdd().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.AccessMember().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.AccessSimple().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.AccessSimple1().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.Assignments().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.AssignmentWithCut().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.ContextOperatorPrint().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.List2().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.List3().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.List2AndEmpty().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.DumpPrint().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.InnerAccess().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.NamedSimpleAssignment().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.SimpleAssignment().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.SimpleAssignment1().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.StrangeStructs().Run);
TestRunner.RunTest(new Reni.FeatureTest.CleanupSection.Simple().Run);
TestRunner.RunTest(new Reni.FeatureTest.CleanupSection.WithReference().Run);
TestRunner.RunTest(new Reni.FeatureTest.BlogExamples.Post120617().Run);
TestRunner.RunTest(new Reni.FeatureTest.BlogExamples.NestedCompound().Run);
TestRunner.RunTest(new Reni.FeatureTest.BlogExamples.WikiExamples().Run);
TestRunner.RunTest(new Reni.FeatureTest.BitArrayOp.BitArrayOp().NegativeNumber);
TestRunner.RunTest(new Reni.FeatureTest.BitArrayOp.BitArrayOp().Run);
TestRunner.RunTest(new Reni.FeatureTest.BitArrayOp.Negate().Run);
TestRunner.RunTest(new Reni.FeatureTest.BitArrayOp.TwoPositiveNumbers().Run);
TestRunner.RunTest(new Reni.FeatureTest.BitArrayOp.PositiveNumbers().Run);
TestRunner.RunTest(new Reni.FeatureTest.BitArrayOp.TwoNegativeNumbers().Run);
TestRunner.RunTest(new Reni.FeatureTest.BitArrayOp.NegativeNumbers().Run);
TestRunner.RunTest(new Reni.FeatureTest.BitArrayOp.Number().Run);
TestRunner.RunTest(new Reni.FeatureTest.BitArrayOp.Add2Numbers().Run);
TestRunner.RunTest(new Reni.FeatureTest.BitArrayOp.SubtractOddSizedNumber().Run);
TestRunner.RunTest(new Reni.FeatureTest.BitArrayOp.AddOddSizedNumber().Run);
TestRunner.RunTest(new Reni.FeatureTest.BitArrayOp.SubtractLargeEqualSizedNumber().Run);
TestRunner.RunTest(new Reni.FeatureTest.BitArrayOp.SubtractLargerSizedNumber().Run);
TestRunner.RunTest(new Reni.FeatureTest.Reference.ArrayElementType1().Run);
TestRunner.RunTest(new Reni.FeatureTest.Reference.ArrayReferenceCopy().Run);
TestRunner.RunTest(new Reni.FeatureTest.Reference.ArrayReferenceDumpLoop().Run);
TestRunner.RunTest(new Reni.FeatureTest.Reference.FunctionalDumpSimple().Run);
TestRunner.RunTest(new Reni.FeatureTest.Reference.ArrayReferenceDumpSimple().Run);
TestRunner.RunTest(new Reni.FeatureTest.Array.ArrayFromPieces().Run);
TestRunner.RunTest(new Reni.FeatureTest.Array.TypeOfArrayFromPieces().Run);
TestRunner.RunTest(new Reni.FeatureTest.Array.CombineArraysFromPieces().Run);
TestRunner.RunTest(new Reni.FeatureTest.Array.TypeOfCombineArraysFromPieces().Run);
TestRunner.RunTest(new Reni.FeatureTest.Array.ElementAccessSimple().Run);
TestRunner.RunTest(new Reni.FeatureTest.Array.ElementAccess().Run);
TestRunner.RunTest(new Reni.FeatureTest.Array.ArrayVariable().Run);
TestRunner.RunTest(new Reni.FeatureTest.Array.ElementAccessVariableSetterSimple().Run);
TestRunner.RunTest(new Reni.FeatureTest.Array.ElementAccessVariableSetter().Run);
TestRunner.RunTest(new Reni.FeatureTest.Array.ElementAccessVariable().Run);
TestRunner.RunTest(new Reni.FeatureTest.Array.FromTypeAndFunction().Run);
TestRunner.RunTest(new Reni.FeatureTest.Array.TypeOfSimpleArrayFromPiece().Run);
TestRunner.RunTest(new Reni.FeatureTest.Array.TypeOfElementOfSimpleArrayFromPiece().Run);
TestRunner.RunTest(new ReniUI.Test.BracketMatching().MatchingBraces);
TestRunner.RunTest(new ReniUI.Test.BracketMatching().MoreMatchingBraces);
TestRunner.RunTest(new ReniUI.Test.BracketMatching().NotMatchingBraces);
TestRunner.RunTest(new ReniUI.Test.ListMatching().Matching);
TestRunner.RunTest(new ReniUI.Test.ListMatching().CombinationsOfMatching);
TestRunner.RunTest(new ReniUI.Test.ListMatching().MixedMatching);
TestRunner.RunTest(new ReniUI.Test.UserInteraction.BadUserInterAction().GetTokenForPosition);
TestRunner.RunTest(new ReniUI.Test.UserInteraction.BadUserInterAction().GetTokenForPositionSimple);
TestRunner.RunTest(new ReniUI.Test.UserInteraction.UserInterAction2().GetTokenForPosition);
TestRunner.RunTest(new ReniUI.Test.Formatting.BadThings().BadArgDeclaration);
TestRunner.RunTest(new ReniUI.Test.Formatting.Complex().Reformat);
TestRunner.RunTest(new ReniUI.Test.Formatting.Complex().Reformat1_120);
TestRunner.RunTest(new ReniUI.Test.Formatting.Complex().Reformat1_120TopLineBreak);
TestRunner.RunTest(new ReniUI.Test.Formatting.Complex().Reformat1_120EmptyBrackets);
TestRunner.RunTest(new ReniUI.Test.Formatting.Complex().Reformat2);
TestRunner.RunTest(new ReniUI.Test.Formatting.Complex().TwoLevelParenthesis);
TestRunner.RunTest(new ReniUI.Test.Formatting.Complex().UseLineBreakBeforeParenthesis);
TestRunner.RunTest(new ReniUI.Test.Formatting.Complex().HalfList);
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

TestRunner.RunTest(new Reni.FeatureTest.Text1().Run);
TestRunner.RunTest(new Reni.FeatureTest.TextConcat().Run);
TestRunner.RunTest(new Reni.FeatureTest.TypeType.TypeOperatorWithVariable().Run);
TestRunner.RunTest(new Reni.FeatureTest.ThenElse.UseThen().Run);
TestRunner.RunTest(new Reni.FeatureTest.ThenElse.UseElse().Run);
TestRunner.RunTest(new Reni.FeatureTest.ThenElse.AutomaticDereferencing().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.AccessAndAddComplex().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.ContextOperatorAccess().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.ContextOperatorFunctionAccess().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.ContextOperatorPropertyAccess().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.ContextOperatorAssign().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.ContextOperator().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.PropertyVariable().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.PropertyVariable().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.SomeVariables().Run);
TestRunner.RunTest(new Reni.FeatureTest.MixIn.Function().Run);
TestRunner.RunTest(new Reni.FeatureTest.MixIn.Simple().Run);
TestRunner.RunTest(new Reni.FeatureTest.Integer.IntegerPlusInteger().Run);
TestRunner.RunTest(new Reni.FeatureTest.Integer.IntegerPlusNumber().Run);
TestRunner.RunTest(new Reni.FeatureTest.Integer.Clone().Run);
TestRunner.RunTest(new Reni.FeatureTest.Integer.Create().Run);
TestRunner.RunTest(new Reni.FeatureTest.Integer.Integer1().Run);
TestRunner.RunTest(new Reni.FeatureTest.Integer.Integer2().Run);
TestRunner.RunTest(new Reni.FeatureTest.Integer.Integer127().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.ConstantFunction().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.Function().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.FunctionAssignment().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.FunctionOfFunction().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.FunctionArgument().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.FunctionVariable().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.FunctionWithNonLocal().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.FunctionWithRefArg().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.FunctionWithRefArg().Run);
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
TestRunner.RunTest(new Reni.FeatureTest.Reference.ArrayReferenceAll().Run);
TestRunner.RunTest(new Reni.FeatureTest.Reference.ArrayReferenceByInstance().Run);
TestRunner.RunTest(new Reni.FeatureTest.Reference.ArrayReferenceCopyAssign().Run);
TestRunner.RunTest(new ReniUI.Test.AutoComplete().GetDeclarationOptions);
TestRunner.RunTest(new ReniUI.Test.AutoCompleteFunctionInCompound().GetDeclarationOptions);
TestRunner.RunTest(new ReniUI.Test.AutoCompleteSimple().GetDeclarationOptions);
TestRunner.RunTest(new ReniUI.Test.Formatting.LongTest().ReformatPart);
}}}
