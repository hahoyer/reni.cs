using System;
using HWClassLibrary.Debug;
using HWClassLibrary.UnitTest;

namespace Reni.FeatureTest.Integer
{
    /// <summary>
    /// Structure, that is all between brackets
    /// </summary>
    public class IntegerStruct : CompilerTest
    {
        protected static string Definition()
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

        public override string Target
        {
            get { return Definition() + "; " + InstanceCode + " dump_print"; }
        }

        protected virtual string InstanceCode { get { return GetStringAttribute<InstanceCodeAttribute>(); } }
    }

    [TestFixture, Output("3"), InstanceCode("(Integer8(1)+Integer8(2))")]
    [IntegerPlusNumber]
    public class IntegerPlusInteger : IntegerStruct
    {
        [Test, Category(UnderConstruction)]
        public override void Run() { BaseRun();}
    }

    [TestFixture, Output("3"), InstanceCode("(Integer8(1)+2)")]
    [Create]
    public class IntegerPlusNumber : IntegerStruct
    {
        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }

    [TestFixture, Output("23"), InstanceCode("Integer8(23) clone")]
    [Create]
    public class Clone : IntegerStruct
    {
        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }

    [TestFixture, Output("23"), InstanceCode("Integer8(0) create(23)")]
    [DumpPrint1, DumpPrint2, DumpPrint127]
    public class Create : IntegerStruct
    {
        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }

    [TestFixture, Output("1"), InstanceCode("Integer8(1)")]
    public class DumpPrint1 : IntegerStruct
    {
        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }

    [TestFixture, Output("2"), InstanceCode("Integer8(2)")]
    public class DumpPrint2 : IntegerStruct
    {
        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }

    [TestFixture, Output("127"), InstanceCode("Integer8(127)")]
    public class DumpPrint127 : IntegerStruct
    {
        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }

}