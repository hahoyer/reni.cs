using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.UnitTest;
using Reni.FeatureTest.DefaultOperations;
using Reni.FeatureTest.Struct;

namespace Reni.FeatureTest.Integer
{
    /// <summary>
    ///     Structure, that is all between brackets
    /// </summary>
    public class IntegerStruct : CompilerTest
    {
        private static string Definition()
        {
            return
                @"
Integer8: 
{
    _data: 127 type (arg enable_cut);

    create   : (Integer8(arg))/\;
    !property 
    dump_print: (_data dump_print)/\ ;
    +        :  create(_data + create(arg) _data)/\;
    !property 
    clone: create(_data)/\ ;
    !property 
    enable_cut: _data enable_cut /\ ;
    !converter: _data /\ ;
}/\
";
        }

        public override void Run() { }
        protected override string Target { get { return Definition() + "; " + InstanceCode + " dump_print"; } }
        protected virtual string InstanceCode { get { return GetStringAttribute<InstanceCodeAttribute>(); } }
    }

    [TestFixture]
    [Output("3")]
    [InstanceCode("(Integer8(1)+Integer8(2))")]
    [IntegerPlusNumber]
    public sealed class IntegerPlusInteger : IntegerStruct
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [Output("3")]
    [InstanceCode("(Integer8(1)+2)")]
    [Create]
    public sealed class IntegerPlusNumber : IntegerStruct
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [Output("23")]
    [InstanceCode("Integer8(23) clone")]
    [Create]
    public sealed class Clone : IntegerStruct
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [Output("23")]
    [InstanceCode("Integer8(0) create(23)")]
    [DumpPrint1]
    [DumpPrint2]
    [DumpPrint127]
    public sealed class Create : IntegerStruct
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [PropertyVariable, ApplyTypeOperatorWithCut]
    [Output("1")]
    [InstanceCode("Integer8(1)")]
    public sealed class DumpPrint1 : IntegerStruct
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [Output("2")]
    [DumpPrint1]
    [InstanceCode("Integer8(2)")]
    public sealed class DumpPrint2 : IntegerStruct
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [DumpPrint2]
    [Output("127")]
    [InstanceCode("Integer8(127)")]
    public sealed class DumpPrint127 : IntegerStruct
    {
        [Test]
        public override void Run() { BaseRun(); }
    }
}