using Reni.Basics;
using Reni.Code;
using Reni.Type;

namespace Reni.Context;

sealed class Function : Child, IFunctionContext
{
    [Node]
    internal TypeBase ArgumentsType { get; }

    readonly int Order;

    [Node]
    TypeBase? ValueType { get; }

    internal Function(ContextBase parent, TypeBase argumentsType, TypeBase? valueType = null)
        : base(parent)
    {
        Order = Closures.NextOrder++;
        ArgumentsType = argumentsType;
        ValueType = valueType;
        StopByObjectIds();
    }

    int IContextReference.Order => Order;

    TypeBase IFunctionContext.ArgumentsType => ArgumentsType;

    Result IFunctionContext.CreateArgumentReferenceResult(Category category) => ArgumentsType
            .GetContextAccessResult(category | Category.Type, this, () => ArgumentsType.Size * -1)
        & category;

    Result IFunctionContext.CreateValueReferenceResult(Category category)
    {
        if(ValueType == null)
            throw new ValueCannotBeUsedHereException();
        return ValueType.Pointer
                .GetContextAccessResult
                (
                    category | Category.Type,
                    this,
                    () => (ArgumentsType.Size + Root.DefaultRefAlignParam.RefSize) * -1)
            & category;
    }

    protected override string ContextChildIdentificationDump => "@(." + ArgumentsType.ObjectId + "i)";

    internal override IFunctionContext GetRecentFunctionContext() => this;

    [DisableDump]
    protected override string LevelFormat => "function";
}

sealed class ValueCannotBeUsedHereException : Exception;

interface IFunctionContext : IContextReference
{
    Result CreateArgumentReferenceResult(Category category);
    Result CreateValueReferenceResult(Category category);
    TypeBase ArgumentsType { get; }
}