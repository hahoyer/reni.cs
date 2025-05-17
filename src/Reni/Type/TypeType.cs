using Reni.Basics;
using Reni.Context;
using Reni.Feature;
using Reni.Numeric;
using Reni.Struct;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.Type;

sealed class TypeType
    : TypeBase
        , ISymbolProvider<DumpPrintToken>
        , ISymbolProvider<Star>
        , ISymbolProvider<Slash>
        , ISymbolProvider<Mutable>
        , ISymbolProvider<ArrayReference>
{
    
    TypeBase Value { get; }

    public TypeType(TypeBase value)
    {
        Value = value;
        StopByObjectIds(83);
    }

    IImplementation? ISymbolProvider<ArrayReference>.Feature
        => Value is ArrayType? Feature.Extension.Value(ArrayReferenceResult) : null;

    IImplementation ISymbolProvider<DumpPrintToken>.Feature => Feature.Extension.Value(GetDumpPrintTokenResult);

    IImplementation? ISymbolProvider<Mutable>.Feature
        => Value is ArrayType? Feature.Extension.Value(MutableArrayResult) :
            Value is ArrayReferenceType? Feature.Extension.Value(MutableReferenceResult) : null;

    IImplementation ISymbolProvider<Slash>.Feature
        => Feature.Extension.MetaFeature(SlashResult!);

    IImplementation ISymbolProvider<Star>.Feature
        => Feature.Extension.MetaFeature(StarResult!);

    [DisableDump]
    internal override Root Root => Value.Root;

    [DisableDump]
    internal override bool IsHollow => true;

    internal override string DumpPrintText => "(" + Value.DumpPrintText + "()) type";

    protected override string GetNodeDump() => "(" + Value.NodeDump + ") type";

    internal override Result GetInstanceResult(Category category, Func<Category, Result> getRightResult)
        => RawInstanceResult(category | Category.Type, getRightResult).LocalReferenceResult;

    Result RawInstanceResult(Category category, Func<Category, Result> getRightResult)
    {
        if(Category.Type.Replenished().Contains(category))
            return Value.GetResult(category | Category.Type);
        var constructorResult = Value
            .GetConstructorResult(category, getRightResult(Category.Type).Type!);
        return constructorResult.ReplaceArguments(getRightResult);
    }

    new Result GetDumpPrintTokenResult(Category category)
        => Value.GetDumpPrintTypeNameResult(category);

    Result StarResult(Category category, ResultCache left, ContextBase context, ValueSyntax right)
    {
        var countResult = right.GetResultForAll(context).AutomaticDereferenceResult;
        var count = countResult
            .GetValue(context.RootContext.ExecutionContext)
            .ToInt32();
        var type = Value
            .Align
            .GetArray(count)
            .TypeType;
        return type.GetResult(category);
    }

    Result SlashResult(Category category, ResultCache left, ContextBase context, ValueSyntax right)
    {
        var rightType = right
            .GetTypeBase(context)
            .GetSmartUn<FunctionType>()
            .GetSmartUn<PointerType>();

        if(rightType is not TypeType rightTypeType)
        {
            NotImplementedMethod(context, category, left, right, "rightType", rightType);
            return null!;
        }

        var count = Value.GetSmartArrayLength(rightTypeType.Value);
        if(count == null)
        {
            NotImplementedMethod(context, category, left, right, "rightType", rightType);
            return null!;
        }

        return Root.BitType.GetResult(category, BitsConst.Convert(count.Value));
    }

    Result MutableArrayResult(Category category)
        => ((ArrayType)Value).Mutable.TypeType.GetResult(category);

    Result ArrayReferenceResult(Category category)
        => ((ArrayType)Value).Reference(true).TypeType.GetResult(category);

    Result MutableReferenceResult(Category category)
        => ((ArrayReferenceType)Value).Mutable.TypeType.GetResult(category);
}