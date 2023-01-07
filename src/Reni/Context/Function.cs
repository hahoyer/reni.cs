using hw.DebugFormatter;
using Reni.Basics;
using Reni.Code;
using Reni.Type;

namespace Reni.Context;

sealed class Function : Child, IFunctionContext
{
    [Node]
    internal TypeBase ArgsType { get; }

    readonly int Order;

    [Node]
    TypeBase ValueType { get; }

    internal Function(ContextBase parent, TypeBase argsType, TypeBase valueType = null)
        : base(parent)
    {
        Order = Closures.NextOrder++;
        ArgsType = argsType;
        ValueType = valueType;
        StopByObjectIds();
    }

    int IContextReference.Order => Order;

    TypeBase IFunctionContext.ArgsType => ArgsType;

    Result IFunctionContext.CreateArgReferenceResult(Category category) => ArgsType
            .ContextAccessResult(category | Category.Type, this, () => ArgsType.Size * -1)
        & category;

    Result IFunctionContext.CreateValueReferenceResult(Category category)
    {
        if(ValueType == null)
            throw new ValueCannotBeUsedHereException();
        return ValueType.Pointer
                .ContextAccessResult
                (
                    category | Category.Type,
                    this,
                    () => (ArgsType.Size + Root.DefaultRefAlignParam.RefSize) * -1)
            & category;
    }

    protected override string GetContextChildIdentificationDump()
        => "@(." + ArgsType.ObjectId + "i)";

    internal override IFunctionContext ObtainRecentFunctionContext() => this;

    [DisableDump]
    protected override string LevelFormat => "function";
}

sealed class ValueCannotBeUsedHereException : Exception { }

interface IFunctionContext : IContextReference
{
    Result CreateArgReferenceResult(Category category);
    Result CreateValueReferenceResult(Category category);
    TypeBase ArgsType { get; }
}