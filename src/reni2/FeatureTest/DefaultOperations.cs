using System;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using HWClassLibrary.UnitTest;
using Reni.FeatureTest.BitArrayOp;
using Reni.FeatureTest.Struct;

namespace Reni.FeatureTest.DefaultOperations
{
    [TestFixture]
    public class TypeOperator : CompilerTest
    {
        protected override string Target { get { return @"31 type dump_print"; } }
        protected override string Output { get { return "(bit)sequence(6)"; } }

        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    public class TypeOperatorWithVariable : CompilerTest
    {
        protected override string Target { get { return @"x: 0; x type dump_print"; } }
        protected override string Output { get { return "(bit)sequence(1)"; } }
        protected override IEnumerable<System.Type> DependsOn { get { return new[] { typeof(SomeVariables), typeof(TypeOperator) }; } }

        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    public class ApplyTypeOperator: CompilerTest
    {
        protected override string Target { get { return @"(31 type (28))dump_print"; } }
        protected override string Output { get { return "28"; } }
        protected override IEnumerable<System.Type> DependsOn { get { return new[] {typeof(TypeOperator)}; } }

        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    public class ApplyTypeOperatorWithCut : CompilerTest
    {
        protected override string Target { get { return @"(31 type (100 enable_cut))dump_print"; } }
        protected override string Output { get { return "-28"; } }
        protected override IEnumerable<System.Type> DependsOn { get { return new[] { typeof(ApplyTypeOperator) }; } }

        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    abstract public class ApplyCompareOperator : CompilerTest
    {
        protected override string Target { get { return "(1"+Operator+"100)dump_print"; } }
        protected abstract string Operator { get; }
        protected abstract bool Result { get; }
        protected override string Output { get { return Result ? "-1" : "0"; } }
        protected override IEnumerable<System.Type> DependsOn { get { return new[] { typeof(Add2Numbers) }; } }

        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }

    public class Equal : ApplyCompareOperator
    {
        protected override string Operator { get { return "="; } }
        protected override bool Result { get { return false; } }
    }
    public class NotEqual : ApplyCompareOperator
    {
        protected override string Operator { get { return "<>"; } }
        protected override bool Result { get { return true; } }
    }
    public class GreaterThan : ApplyCompareOperator
    {
        protected override string Operator { get { return ">"; } }
        protected override bool Result { get { return false; } }
    }
    public class LessThan : ApplyCompareOperator
    {
        protected override string Operator { get { return "<"; } }
        protected override bool Result { get { return true; } }
    }
    public class LessOrEqual : ApplyCompareOperator
    {
        protected override string Operator { get { return "<="; } }
        protected override bool Result { get { return true; } }
    }
    public class GreaterOrEqual : ApplyCompareOperator
    {
        protected override string Operator { get { return ">="; } }
        protected override bool Result { get { return false; } }
    }
}