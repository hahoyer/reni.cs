using System;
using HWClassLibrary.Debug;
using NUnit.Framework;

namespace Reni.FeatureTest.Integer
{
    /// <summary>
    /// Structure, that is all between brackets
    /// </summary>
    public class IntegerStruct : CompilerTest
    {
        protected static string IntegerDefinition()
        {
            return
                @"
Integer8: function
{
    _data: 127 type (arg enable_cut);

    create   : function(Integer8(arg));
    !property dump_print: function (_data dump_print);
    +        : function create(_data + create(arg) _data);
    !property clone: function create(_data);
    !property enable_cut: function _data enable_cut;
    !converter: function _data
}
";
        }

        public override void Run() { }

        public override string Target
        {
            get { return IntegerDefinition() + "; " + InstanceCode + " dump_print"; }
        }

        protected virtual string InstanceCode { get { return GetStringAttribute<InstanceCodeAttribute>(); } }
    }

    [TestFixture, Output("3"), InstanceCode("(Integer8(1)+Integer8(2))")]
    public class Plus : IntegerStruct
    {
        [Test, Category(Worked)]
        public override void Run() { BaseRun();}
    }

    [TestFixture, Output("23"), InstanceCode("Integer8(23) clone")]
    public class Clone : IntegerStruct
    {
        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }

    [TestFixture, Output("23"), InstanceCode("Integer8(0) create(23)")]
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

    [AttributeUsage(AttributeTargets.Class)]
    internal sealed class InstanceCodeAttribute : StringAttribute
    {
        public InstanceCodeAttribute(string value)
            : base(value) { }
    }

}