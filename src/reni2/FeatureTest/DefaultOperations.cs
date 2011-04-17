using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.UnitTest;
using Reni.FeatureTest.BitArrayOp;
using Reni.FeatureTest.Struct;

namespace Reni.FeatureTest.DefaultOperations
{
    [TestFixture]
    public sealed class TypeOperator : CompilerTest
    {
        protected override string Target { get { return @"31 type dump_print"; } }
        protected override string Output { get { return "(bit)sequence(6)"; } }

        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    public sealed class TypeOperatorWithVariable : CompilerTest
    {
        protected override string Target { get { return @"x: 0; x type dump_print"; } }
        protected override string Output { get { return "(bit)sequence(1)"; } }
        protected override IEnumerable<System.Type> DependsOn { get { return new[] {typeof(SomeVariables), typeof(TypeOperator)}; } }

        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    public sealed class ApplyTypeOperator : CompilerTest
    {
        protected override string Target { get { return @"(31 type (28))dump_print"; } }
        protected override string Output { get { return "28"; } }
        protected override IEnumerable<System.Type> DependsOn { get { return new[] {typeof(TypeOperator)}; } }

        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    public sealed class ApplyTypeOperatorWithCut : CompilerTest
    {
        protected override string Target { get { return @"(31 type (100 enable_cut))dump_print"; } }
        protected override string Output { get { return "-28"; } }
        protected override IEnumerable<System.Type> DependsOn { get { return new[] {typeof(ApplyTypeOperator)}; } }

        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    public abstract class ApplyCompareOperator : CompilerTest
    {
        protected override string Target { get { return "(1" + Operator + "100)dump_print"; } }
        protected abstract string Operator { get; }
        protected abstract bool Result { get; }
        protected override string Output { get { return Result ? "-1" : "0"; } }
        protected override IEnumerable<System.Type> DependsOn { get { return new[] {typeof(Add2Numbers)}; } }

        [Test]
        public override void Run() { BaseRun(); }
    }

    public sealed class Equal : ApplyCompareOperator
    {
        protected override string Operator { get { return "="; } }
        protected override bool Result { get { return false; } }
    }

    public sealed class NotEqual : ApplyCompareOperator
    {
        protected override string Operator { get { return "<>"; } }
        protected override bool Result { get { return true; } }
    }

    public sealed class GreaterThan : ApplyCompareOperator
    {
        protected override string Operator { get { return ">"; } }
        protected override bool Result { get { return false; } }
    }

    public sealed class LessThan : ApplyCompareOperator
    {
        protected override string Operator { get { return "<"; } }
        protected override bool Result { get { return true; } }
    }

    public sealed class LessOrEqual : ApplyCompareOperator
    {
        protected override string Operator { get { return "<="; } }
        protected override bool Result { get { return true; } }
    }

    public sealed class GreaterOrEqual : ApplyCompareOperator
    {
        protected override string Operator { get { return ">="; } }
        protected override bool Result { get { return false; } }
    }
}