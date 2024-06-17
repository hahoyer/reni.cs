using hw.DebugFormatter;
using hw.Helper;
using hw.UnitTest;
using Reni.FeatureTest.Array;
using Reni.FeatureTest.BlogExamples;
using Reni.FeatureTest.Function;
using Reni.FeatureTest.Helper;
using Reni.FeatureTest.Integer;
using Reni.FeatureTest.MixIn;
using Reni.FeatureTest.Reference;
using Reni.FeatureTest.Structure;
using Reni.FeatureTest.Text;
using Reni.FeatureTest.ThenElse;
using Reni.FeatureTest.TypeType;
using Reni.FeatureTest.Validation;

namespace Reni.FeatureTest;

public abstract class TextStruct : CompilerTest
{
    protected virtual string InstanceCode => GetStringAttribute<InstanceCodeAttribute>();
    protected override string Target => Definition() + "; (" + InstanceCode + ") dump_print";
    static string Definition() => (SmbFile.SourceFolder / "Text.reni").String;
}

[UnitTest]
[Output("abcdef")]
[InstanceCode("Text ('abcdef')")]
[Integer1]
[TwoFunctions]
[FromTypeAndFunction]
[HalloWelt]
[ElementAccess]
[ElementAccessVariableSetter]
[TypeOperator]
[DefaultInitialized]
[FunctionVariable]
[WikiExamples]
[Repeater]
[FunctionArgument]
[PrimitiveRecursiveFunctionHuge]
[ArrayElementType]
[ArrayReferenceAll]
[Simple]
[MixIn.Function]
public sealed class Text1 : TextStruct;

[UnitTest]
[Output("Hallo")]
[InstanceCode("Text ('H') << 'allo'")]
[Text1]
[UserObjects]
[UnMatchedBrackets]
[AutomaticDereferencing]
[ContextOperator]
[ComplexContext]
public sealed class TextConcat : TextStruct
{
    public TextConcat() => Parameters.Semantics = true;

    protected override void AssertValid(Compiler compiler)
    {
        compiler.Syntax.Semantics.Information.Log();
        true.ConditionalBreak();
    }
}