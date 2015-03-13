using System;
using System.Collections.Generic;
using System.Linq;
using hw.UnitTest;
using Reni.FeatureTest.BitArrayOp;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.DefaultOperations
{
    [TestFixture]
    [ConversionService.Closure]
    public abstract class ApplyCompareOperator : CompilerTest
    {
        protected override string Target => "(1" + Operator + "100)dump_print";
        protected abstract string Operator { get; }
        protected abstract bool Result { get; }
        protected override string Output => Result ? "-1" : "0";
        protected override IEnumerable<System.Type> DependsOn => new[] {typeof(Add2Numbers)};
    }

    public sealed class Equal : ApplyCompareOperator
    {
        protected override string Operator => "=";
        protected override bool Result => false;
    }

    public sealed class NotEqual : ApplyCompareOperator
    {
        protected override string Operator => "<>";
        protected override bool Result => true;
    }

    public sealed class GreaterThan : ApplyCompareOperator
    {
        protected override string Operator => ">";
        protected override bool Result => false;
    }

    public sealed class LessThan : ApplyCompareOperator
    {
        protected override string Operator => "<";
        protected override bool Result => true;
    }

    public sealed class LessOrEqual : ApplyCompareOperator
    {
        protected override string Operator => "<=";
        protected override bool Result => true;
    }

    public sealed class GreaterOrEqual : ApplyCompareOperator
    {
        protected override string Operator => ">=";
        protected override bool Result => false;
    }
}