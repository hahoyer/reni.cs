using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Helper;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.Type;

sealed class TextItemType
    : Child<ArrayType>
        , ISymbolProviderForPointer<DumpPrintToken>
        , ISymbolProviderForPointer<ToNumberOfBase>
{
    public TextItemType(ArrayType parent)
        : base(parent)
        => (!parent.OverView.IsHollow).Assert();

    IImplementation ISymbolProviderForPointer<DumpPrintToken>.Feature
        => Feature.Extension.Value(GetDumpPrintTokenResult);

    IImplementation? ISymbolProviderForPointer<ToNumberOfBase>.Feature
        => Feature.Extension.MetaFeature(ToNumberOfBaseResult);

    protected override Result ParentConversionResult(Category category)
    {
        NotImplementedMethod(category);
        return default!;
    }

    [DisableDump]
    protected override CodeBase DumpPrintCode
        => Make.ArgumentCode.GetDumpPrintText(Parent.OverView.Size);

    Result ToNumberOfBaseResult(Category category, ResultCache left, ContextBase context, ValueSyntax? right)
    {
        var target = (left & Category.All).AutomaticDereferencedAligned
            .GetValue(context.RootContext.ExecutionContext);
        //.ToString(ElementType.Size);
        //todo: Error handling:
        var conversionBase = right!.Evaluate(context).ExpectNotNull().ToInt32();
        (conversionBase >= 2).Expect(() => (right.MainAnchor.SourcePart, conversionBase.ToString()));
        var result = BitsConst.Convert((string)target, conversionBase);
        return Root.BitType.GetResult(category, result).Aligned;
    }
}
