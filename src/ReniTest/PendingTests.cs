//02:08:52.791 04.12.2021 ran 150 of 182 

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
TestRunner.RunTest(new Reni.FeatureTest.Integer.Integer1().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.SimpleAssignment().Run);
TestRunner.RunTest(new Reni.FeatureTest.MixIn.Simple().Run);
TestRunner.RunTest(new Reni.FeatureTest.CleanupSection.Nested().Run);
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
TestRunner.RunTest(new Reni.FeatureTest.UserObjects().Run);
TestRunner.RunTest(new Reni.FeatureTest.Integer.IntegerPlusInteger().Run);
TestRunner.RunTest(new Reni.FeatureTest.Integer.IntegerPlusNumber().Run);
TestRunner.RunTest(new Reni.FeatureTest.Integer.Clone().Run);
TestRunner.RunTest(new Reni.FeatureTest.Integer.Create().Run);
TestRunner.RunTest(new Reni.FeatureTest.Integer.Integer2().Run);
TestRunner.RunTest(new Reni.FeatureTest.Integer.Integer127().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.Assignments().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.AssignmentWithCut().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.NamedSimpleAssignment().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.SimpleAssignment1().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.ContextOperatorAccess().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.ContextOperatorFunctionAccess().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.ContextOperatorPropertyAccess().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.ContextOperatorAssign().Run);
TestRunner.RunTest(new Reni.FeatureTest.Structure.ContextOperator().Run);
TestRunner.RunTest(new Reni.FeatureTest.MixIn.Function().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.PrimitiveRecursiveFunctionByteWithDump().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.PrimitiveRecursiveFunctionHuge().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.PrimitiveRecursiveFunctionSmall().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.PrimitiveRecursiveFunctionWithDump().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.SimpleRepeater().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.Repeater().Run);
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
TestRunner.RunTest(new ReniUI.Test.Formatting.LongTest().ReformatPart);
}}}
