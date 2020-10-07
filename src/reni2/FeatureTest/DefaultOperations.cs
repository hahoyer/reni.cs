using System.Collections.Generic;
using hw.UnitTest;
using Reni.FeatureTest.BitArrayOp;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.DefaultOperations
{
    [UnitTest]
    [ConversionService.Closure]
    public abstract class ApplyCompareOperator : CompilerTest
    {
        protected override string Target => "(1" + Operator + "100)dump_print";
        protected abstract string Operator { get; }
        protected abstract bool Result { get; }
        protected override string Output => Result ? "-1" : "0";
        protected virtual IEnumerable<System.Type> DependsOn => new[] {typeof(Add2Numbers)};
    }

    [UnitTest]
    public sealed class Equal : ApplyCompareOperator
    {
        protected override string Operator => "=";
        protected override bool Result => false;
    }

    [UnitTest]
    public sealed class NotEqual : ApplyCompareOperator
    {
        protected override string Operator => "<>";
        protected override bool Result => true;
    }

    [UnitTest]
    public sealed class GreaterThan : ApplyCompareOperator
    {
        protected override string Operator => ">";
        protected override bool Result => false;
    }

    [UnitTest]
    public sealed class LessThan : ApplyCompareOperator
    {
        protected override string Operator => "<";
        protected override bool Result => true;
    }

    [UnitTest]
    public sealed class LessOrEqual : ApplyCompareOperator
    {
        protected override string Operator => "<=";
        protected override bool Result => true;
    }

    [UnitTest]
    public sealed class GreaterOrEqual : ApplyCompareOperator
    {
        protected override string Operator => ">=";
        protected override bool Result => false;
    }
}