//00:36:18.947 31.12.2022 ran 163 of 189 

// ReSharper disable once CheckNamespace
namespace hw.UnitTest;
public static class PendingTests
{
    public static void Run()
    {
    
// error 

TestRunner.RunTest(new Reni.FeatureTest.PublicNonPublic1().Run);
TestRunner.RunTest(new Reni.FeatureTest.ThenElse.UseThen().Run);
TestRunner.RunTest(new Reni.FeatureTest.ThenElse.UseElse().Run);
TestRunner.RunTest(new Reni.FeatureTest.Validation.UndefinedContextSymbol().Run);
TestRunner.RunTest(new Reni.FeatureTest.Validation.UndefinedSymbol().Run);
TestRunner.RunTest(new ReniUI.Test.StrangeExpressions().StringPrefix);
TestRunner.RunTest(new ReniUI.Test.Examples.Enum().Start);

// dependanterror 

TestRunner.RunTest(new Reni.FeatureTest.ComplexContext().Run);
TestRunner.RunTest(new Reni.FeatureTest.AllScopeHandling().Run);
TestRunner.RunTest(new Reni.FeatureTest.UserObjects().Run);
TestRunner.RunTest(new Reni.FeatureTest.ThenElse.AutomaticDereferencing().Run);
TestRunner.RunTest(new Reni.FeatureTest.Validation.SyntaxErrorComment().Run);
TestRunner.RunTest(new Reni.FeatureTest.Validation.UseOfUndefinedContextSymbol().Run);
TestRunner.RunTest(new Reni.FeatureTest.Validation.IndirectUseOfUndefinedContextSymbol().Run);
TestRunner.RunTest(new Reni.FeatureTest.MixIn.Function().Run);
TestRunner.RunTest(new Reni.FeatureTest.MixIn.Simple().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.PrimitiveRecursiveFunctionByteWithDump().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.PrimitiveRecursiveFunctionHuge().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.PrimitiveRecursiveFunctionSmall().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.PrimitiveRecursiveFunctionWithDump().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.RecursiveFunction().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.SimpleRepeater().Run);
TestRunner.RunTest(new Reni.FeatureTest.Function.Repeater().Run);
TestRunner.RunTest(new Reni.FeatureTest.Reference.ArrayReferenceAll().Run);
TestRunner.RunTest(new Reni.FeatureTest.Reference.ArrayReferenceByInstance().Run);
TestRunner.RunTest(new Reni.FeatureTest.Reference.ArrayReferenceCopy().Run);
TestRunner.RunTest(new Reni.FeatureTest.Reference.ArrayReferenceCopyAssign().Run);
TestRunner.RunTest(new Reni.FeatureTest.Reference.ArrayReferenceDumpLoop().Run);
TestRunner.RunTest(new Reni.FeatureTest.Reference.FunctionalDumpSimple().Run);
TestRunner.RunTest(new Reni.FeatureTest.Reference.ArrayReferenceDumpSimple().Run);

// notrun 

TestRunner.RunTest(new Reni.FeatureTest.Text1().Run);
TestRunner.RunTest(new Reni.FeatureTest.TextConcat().Run);
TestRunner.RunTest(new ReniUI.Test.Formatting.LongTest().ReformatPart);
}}
