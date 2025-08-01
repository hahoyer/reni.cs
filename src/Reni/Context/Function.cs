using hw.Scanner;
using Reni.Basics;
using Reni.Code;
using Reni.SyntaxTree;
using Reni.Type;

namespace Reni.Context;

sealed class Function : Child, IFunctionContext
{
    readonly SourcePart Token;

    [Node]
    internal TypeBase ArgumentsType { get; }

    readonly int Order;

    [Node]
    TypeBase? ValueType { get; }

    internal Function(ContextBase parent, SourcePart token, TypeBase argumentsType, TypeBase? valueType = null)
        : base(parent)
    {
        Token = token;
        Order = Closures.NextOrder++;
        ArgumentsType = argumentsType;
        ValueType = valueType;
        StopByObjectIds();
    }

    int IContextReference.Order => Order;

    TypeBase IFunctionContext.ArgumentsType => ArgumentsType;

    Result IFunctionContext.CreateArgumentReferenceResult(Category category) => ArgumentsType
            .GetContextAccessResult(category | Category.Type, this, () => ArgumentsType.OverView.Size * -1)
        & category;

    Result IFunctionContext.CreateValueReferenceResult(Category category)
    {
        if(ValueType == null)
            throw new ValueCannotBeUsedHereException();

        return (ValueType.OverView.IsHollow? ValueType : ValueType.Make.Pointer)
            .GetContextAccessResult
            (
                category | Category.Type,
                this,
                () => (ArgumentsType.OverView.Size + Root.DefaultRefAlignParam.RefSize) * -1)
            & category;
    }

    protected override string GetContextIdentificationDumpAsChild() => "@(" + ArgumentsType.NameDump + ")";
    protected override SourcePosition GetMainPosition() => Token.Start;

    internal override IFunctionContext GetRecentFunctionContext() => this;

    internal override string GetPositionInformation(SourcePosition target) => "";

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
