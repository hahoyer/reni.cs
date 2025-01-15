using hw.UnitTest;
using Reni.FeatureTest.BitArrayOp;
using Reni.FeatureTest.ConversionService;
using Reni.FeatureTest.Helper;

// ReSharper disable once CheckNamespace
namespace Reni.FeatureTest.DefaultOperations;

[UnitTest]
[Closure]
public abstract class ApplyCompareOperator : CompilerTest
{
    protected abstract string Operator { get; }
    protected abstract bool Result { get; }
    protected virtual IEnumerable<System.Type> DependsOn => [typeof(Add2Numbers)];
    protected override string Target => "(1" + Operator + "100)dump_print";
    protected override string Output => Result? "-1" : "0";
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
    protected override string Operator => "~=";
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