using System;
using HWClassLibrary.Debug;
using NUnit.Framework;
using Reni.FeatureTest.BitArrayOp;
using Reni.FeatureTest.Struct;

namespace Reni.FeatureTest.DefaultOperations
{
    [TestFixture]
    public class TypeOperator : CompilerTest
    {
        public override string Target { get { return @"31 type dump_print"; } }
        public override string Output { get { return "(bit)sequence(6)"; } }

        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    public class TypeOperatorWithVariable : CompilerTest
    {
        public override string Target { get { return @"x: 0; x type dump_print"; } }
        public override string Output { get { return "(bit)sequence(1)"; } }
        public override System.Type[] DependsOn { get { return new[] { typeof(SomeVariables), typeof(TypeOperator) }; } }

        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    public class ApplyTypeOperator: CompilerTest
    {
        public override string Target { get { return @"(31 type (28))dump_print"; } }
        public override string Output { get { return "28"; } }
        public override System.Type[] DependsOn { get { return new[] {typeof(TypeOperator)}; } }

        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    public class ApplyTypeOperatorWithCut : CompilerTest
    {
        public override string Target { get { return @"(31 type (100 enable_cut))dump_print"; } }
        public override string Output { get { return "-28"; } }
        public override System.Type[] DependsOn { get { return new[] { typeof(ApplyTypeOperator) }; } }

        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    abstract public class ApplyCompareOperator : CompilerTest
    {
        public override string Target { get { return "(1"+Operator+"100)dump_print"; } }
        internal abstract string Operator { get; }
        internal abstract bool Result { get; }
        public override string Output { get { return Result ? "-1" : "0"; } }
        public override System.Type[] DependsOn { get { return new[] { typeof(Add2Numbers) }; } }

        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }

    public class Equal : ApplyCompareOperator
    {
        internal override string Operator { get { return "="; } }
        internal override bool Result { get { return false; } }
    }
    public class NotEqual : ApplyCompareOperator
    {
        internal override string Operator { get { return "<>"; } }
        internal override bool Result { get { return true; } }
    }
    public class GreaterThan : ApplyCompareOperator
    {
        internal override string Operator { get { return ">"; } }
        internal override bool Result { get { return false; } }
    }
    public class LessThan : ApplyCompareOperator
    {
        internal override string Operator { get { return "<"; } }
        internal override bool Result { get { return true; } }
    }
    public class LessOrEqual : ApplyCompareOperator
    {
        internal override string Operator { get { return "<="; } }
        internal override bool Result { get { return true; } }
    }
    public class GreaterOrEqual : ApplyCompareOperator
    {
        internal override string Operator { get { return ">="; } }
        internal override bool Result { get { return false; } }
    }
}