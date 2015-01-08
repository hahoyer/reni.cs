using System;
using System.Collections.Generic;
using System.Linq;
using hw.UnitTest;
using Reni.FeatureTest.BitArrayOp;

namespace Reni.FeatureTest.DefaultOperations
{
    [TestFixture]
    [ConversionService.Test]
    public abstract class ApplyCompareOperator : CompilerTest
    {
        protected override string Target { get { return "(1" + Operator + "100)dump_print"; } }
        protected abstract string Operator { get; }
        protected abstract bool Result { get; }
        protected override string Output { get { return Result ? "-1" : "0"; } }
        protected override IEnumerable<System.Type> DependsOn { get { return new[] {typeof(Add2Numbers)}; } }
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