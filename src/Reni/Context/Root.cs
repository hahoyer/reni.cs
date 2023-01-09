using hw.DebugFormatter;
using hw.Helper;
using Reni.Basics;
using Reni.Code;
using Reni.Feature;
using Reni.Numeric;
using Reni.Parser;
using Reni.Struct;
using Reni.SyntaxTree;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Context;

sealed class Root
    : ContextBase, ISymbolProviderForPointer<Minus>, ISymbolProviderForPointer<ConcatArrays>
{
    internal interface IParent
    {
        bool ProcessErrors { get; }
        IExecutionContext ExecutionContext { get; }
        IEnumerable<Definable> DefinedNames { get; }
        Result<ValueSyntax> ParsePredefinedItem(string source);
    }

    internal static readonly VoidType VoidType = new();

    [DisableDump]
    [Node]
    readonly FunctionList Functions = new();

    [DisableDump]
    [Node]
    readonly IParent Parent;

    public IExecutionContext ExecutionContext => Parent.ExecutionContext;

    [DisableDump]
    [Node]
    internal BitType BitType => this.CachedValue(() => new BitType(this));

    [DisableDump]
    [Node]
    internal RecursionType RecursionType => this.CachedValue(() => new RecursionType(this));

    [DisableDump]
    internal int FunctionCount => Functions.Count;

    internal static RefAlignParam DefaultRefAlignParam => new(BitsConst.SegmentAlignBits, Size.Create(64));

    [DisableDump]
    public bool ProcessErrors => Parent.ProcessErrors;

    [DisableDump]
    internal IEnumerable<Definable> DefinedNames => Parent.DefinedNames;

    internal Root(IParent parent) => Parent = parent;

    IImplementation ISymbolProviderForPointer<ConcatArrays>.Feature(ConcatArrays tokenClass)
        => tokenClass.IsMutable
            ? this.CachedValue(() => GetCreateArrayFeature(true))
            : this.CachedValue(() => GetCreateArrayFeature(false));

    IImplementation ISymbolProviderForPointer<Minus>.Feature(Minus tokenClass)
        => this.CachedValue(GetMinusFeature);

    [DisableDump]
    internal override Root RootContext => this;

    [DisableDump]
    internal override bool IsRecursionMode => false;

    [DisableDump]
    protected override string LevelFormat => "root context";

    public override string GetContextIdentificationDump() => "r";

    static ContextMetaFunction GetCreateArrayFeature(bool isMutable) => new(
        (context, category, argsType) => context.CreateArrayResult(category, argsType, isMutable)
    );

    IImplementation GetMinusFeature()
    {
        var metaDictionary = new FunctionCache<string, ValueSyntax>(CreateMetaDictionary);
        return new ContextMetaFunctionFromSyntax
            (metaDictionary[ArgToken.TokenId + " " + Negate.TokenId]);
    }

    ValueSyntax CreateMetaDictionary(string source)
    {
        var result = Parent.ParsePredefinedItem(source);
        (!result.Issues.Any()).Assert();
        return result.Target;
    }

    internal FunctionType FunctionInstance(CompoundView compoundView, FunctionSyntax body, TypeBase argsType)
    {
        var alignedArgsType = argsType.Align;
        var functionInstance = Functions.Find(body, compoundView, alignedArgsType);
        return functionInstance;
    }

    internal IEnumerable<FunctionType> FunctionInstances(CompoundView compoundView, FunctionSyntax body)
        => Functions.Find(body, compoundView);

    internal Result ConcatPrintResult(Category category, int count, Func<Category, int, Result> elemResults)
    {
        var result = VoidType.Result(category);
        if(!(category.HasCode() || category.HasClosures()))
            return result;

        StartMethodDump(false, category, count, "elemResults");
        try
        {
            if(category.HasCode())
                result.Code = CodeBase.DumpPrintText("(");

            for(var i = 0; i < count; i++)
            {
                Dump("i", i);

                var elemResult = elemResults(category, i);

                Dump("elemResult", elemResult);
                BreakExecution();

                result.IsDirty = true;
                if(category.HasCode())
                {
                    if(i > 0)
                        result.Code = result.Code + CodeBase.DumpPrintText(", ");
                    result.Code = result.Code + elemResult.Code;
                }

                if(category.HasClosures())
                    result.Closures = result.Closures.Sequence(elemResult.Closures);
                result.IsDirty = false;

                Dump("result", result);
                BreakExecution();
            }

            if(category.HasCode())
                result.Code = result.Code + CodeBase.DumpPrintText(")");
            return ReturnMethodDump(result);
        }
        finally
        {
            EndMethodDump();
        }
    }

    internal FunctionContainer FunctionContainer(int index) => Functions.Container(index);
    internal FunctionType Function(int index) => Functions.Item(index);

    internal Container MainContainer(ValueSyntax syntax, string description)
    {
        var rawResult = syntax.Result(this);

        var result = rawResult
            .Code?
            .LocalBlock(rawResult.Type.Copier(Category.Code).Code)
            .Align();

        return new(result, rawResult.Issues.ToArray(), description);
    }
}