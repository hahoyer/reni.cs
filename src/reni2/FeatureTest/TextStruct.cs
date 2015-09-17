using System;
using System.Collections.Generic;
using System.Linq;
using hw.Helper;
using hw.UnitTest;
using Reni.FeatureTest.Array;
using Reni.FeatureTest.BlogExamples;
using Reni.FeatureTest.Function;
using Reni.FeatureTest.Helper;
using Reni.FeatureTest.Integer;
using Reni.FeatureTest.MixIn;
using Reni.FeatureTest.Reference;
using Reni.FeatureTest.Text;
using Reni.FeatureTest.TypeType;
using Reni.FeatureTest.Validation;

namespace Reni.FeatureTest
{
    public class TextStruct : CompilerTest
    {
        static string Definition() => "FeatureTest\\Text.reni".FileHandle().String;
        protected override string Target => Definition() + "; (" + InstanceCode + ") dump_print";
        protected virtual string InstanceCode => GetStringAttribute<InstanceCodeAttribute>();
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
    public sealed class Text1 : TextStruct {}

    [UnitTest]
    [Output("Hallo")]
    [InstanceCode("Text ('H') << 'allo'")]
    [Text1]
    [UserObjects]
    [UnMatchedBrackets]
    [LowPriority]
    public sealed class TextConcat : TextStruct {}
}