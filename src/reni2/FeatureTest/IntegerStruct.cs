using System;
using HWClassLibrary.Debug;
using NUnit.Framework;

namespace Reni.FeatureTest.Integer
{
    /// <summary>
    /// Structure, that is all between brackets
    /// </summary>
    [TestFixture]
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
    clone    : function create(_data);
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

    [TestFixture, Output("2"), InstanceCode("(Integer8(1)+Integer8(2))")]
    public class Plus : IntegerStruct
    {
        [Test, Explicit, Category(UnderConstruction)]
        public override void Run() { }
    }

    [TestFixture, Output("23"), InstanceCode("Integer8(0) clone(23)")]
    public class Clone : IntegerStruct
    {
        [Test, Explicit, Category(UnderConstruction)]
        public override void Run() { }
    }

    [TestFixture, Output("23"), InstanceCode("Integer8(0) create(23)")]
    public class Create : IntegerStruct
    {
        [Test, Explicit, Category(UnderConstruction)]
        public override void Run() { }
    }

    [TestFixture, Output("1"), InstanceCode("Integer8(1)")]
    public class DumpPrint1 : IntegerStruct
    {
        [Test, Explicit, Category(UnderConstruction)]
        public override void Run() { }
    }

    [TestFixture, Output("2"), InstanceCode("Integer8(2)")]
    public class DumpPrint2 : IntegerStruct
    {
        [Test, Explicit, Category(UnderConstruction)]
        public override void Run() { }
    }

    [TestFixture, Output("127"), InstanceCode("Integer8(127)")]
    public class DumpPrint127 : IntegerStruct
    {
        [Test, Explicit, Category(UnderConstruction)]
        public override void Run() { }
    }

    [AttributeUsage(AttributeTargets.Class)]
    internal sealed class InstanceCodeAttribute : StringAttribute
    {
        public InstanceCodeAttribute(string value)
            : base(value) { }
    }

}