using System;
using System.Collections.Generic;
using System.Linq;
using hw.UnitTest;
using Reni.FeatureTest.Function;
using Reni.FeatureTest.Structure;
using Reni.FeatureTest.TypeType;

namespace Reni.FeatureTest.Integer
{
    /// <summary>
    ///     Structure, that is all between brackets
    /// </summary>
    public class IntegerStruct : CompilerTest
    {
        static string Definition()
        {
            return
                @"
Integer8: 
/\{
    _data: 127 type instance(^ enable_cut);

    create   : /\(Integer8(^));
    dump_print: /\(_data dump_print);
    +        :  /\create(_data + create(^) _data);
    clone: /!\create(_data) ;
    enable_cut: /!\_data enable_cut;
    !converter: /\ _data ;
}
";
        }

        protected override string Target { get { return Definition() + "; " + InstanceCode + " dump_print()"; } }
        protected virtual string InstanceCode { get { return GetStringAttribute<InstanceCodeAttribute>(); } }
    }

    [TestFixture]
    [Output("3")]
    [InstanceCode("(Integer8(1)+Integer8(2))")]
    [IntegerPlusNumber]
    [ConversionService.Closure]
    public sealed class IntegerPlusInteger : IntegerStruct {}

    [TestFixture]
    [Output("3")]
    [InstanceCode("(Integer8(1)+2)")]
    [Create]
    [ObjectFunction]
    [TwoFunctions1]
    //[LowPriority]
    public sealed class IntegerPlusNumber : IntegerStruct {}

    [TestFixture]
    [Output("23")]
    [InstanceCode("Integer8(23) clone")]
    [Create]
    [TwoFunctions1]
    public sealed class Clone : IntegerStruct {}

    [TestFixture]
    [Output("23")]
    [InstanceCode("Integer8(0) create(23)")]
    [Integer1]
    [Integer2]
    [Integer127]
    public sealed class Create : IntegerStruct {}

    [TestFixture]
    [PropertyVariable]
    [ApplyTypeOperatorWithCut]
    [SimpleFunctionWithNonLocal]
    [ObjectProperty]
    [Output("1")]
    [InstanceCode("Integer8(1)")]
    public sealed class Integer1 : IntegerStruct {}

    [TestFixture]
    [Output("2")]
    [Integer1]
    [InstanceCode("Integer8(2)")]
    public sealed class Integer2 : IntegerStruct {}

    [TestFixture]
    [Integer2]
    [Output("127")]
    [InstanceCode("Integer8(127)")]
    public sealed class Integer127 : IntegerStruct {}
}