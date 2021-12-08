using hw.UnitTest;
using Reni.FeatureTest.Function;
using Reni.FeatureTest.Helper;
using Reni.FeatureTest.Structure;
using Reni.FeatureTest.TypeType;

// ReSharper disable once CheckNamespace
namespace Reni.FeatureTest.Integer
{
    /// <summary>
    ///     Structure, that is all between brackets
    /// </summary>
    public class IntegerStruct : CompilerTest
    {
        static string Definition() => @"
Integer8: 
@{
    !public _data: 127 type instance(^ enable_cut);

    !public create   : @(Integer8(^));
    !public dump_print: @(_data dump_print);
    !public +        :  @create(_data + create(^) _data);
    !public clone: @!create(_data) ;
    !public enable_cut: @!_data enable_cut;
    !converter: @ _data ;
}
";

        protected override string Target => Definition() + "; " + InstanceCode + " dump_print()";
        protected virtual string InstanceCode => GetStringAttribute<InstanceCodeAttribute>();
    }

    [UnitTest]
    [Output("3")]
    [InstanceCode("(Integer8(1)+Integer8(2))")]
    [IntegerPlusNumber]
    [ConversionService.Closure]
    public sealed class IntegerPlusInteger : IntegerStruct {}

    [UnitTest]
    [Output("3")]
    [InstanceCode("(Integer8(1)+2)")]
    [Create]
    [ObjectFunction]
    [TwoFunctions1]
    //[LowPriority]
    public sealed class IntegerPlusNumber : IntegerStruct {}

    [UnitTest]
    [Output("23")]
    [InstanceCode("Integer8(23) clone")]
    [Create]
    [TwoFunctions1]
    public sealed class Clone : IntegerStruct {}

    [UnitTest]
    [Output("23")]
    [InstanceCode("Integer8(0) create(23)")]
    [Integer1]
    [Integer2]
    [Integer127]
    public sealed class Create : IntegerStruct {}

    [UnitTest]
    [PropertyVariable]
    [ApplyTypeOperatorWithCut]
    [SimpleFunctionWithNonLocal]
    [ObjectProperty]
    [Output("1")]
    [InstanceCode("Integer8(1)")]
    public sealed class Integer1 : IntegerStruct {}

    [UnitTest]
    [Output("2")]
    [Integer1]
    [InstanceCode("Integer8(2)")]
    public sealed class Integer2 : IntegerStruct {}

    [UnitTest]
    [Integer2]
    [Output("127")]
    [InstanceCode("Integer8(127)")]
    public sealed class Integer127 : IntegerStruct {}
}