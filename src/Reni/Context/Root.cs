using Reni.Basics;
using Reni.Code;
using Reni.Feature;
using Reni.Helper;
using Reni.Numeric;
using Reni.Parser;
using Reni.Struct;
using Reni.SyntaxTree;
using Reni.TokenClasses;
using Reni.Type;
using Reni.Validation;

namespace Reni.Context;

sealed class Root
    : ContextBase
        , ISymbolProviderForPointer<Minus>
        , ISymbolProviderForPointer<ConcatArrays>
        , ISymbolProviderForPointer<MutableConcatArrays>
        , ISymbolProviderForPointer<ForeignCode>
        , ISymbolProvider<ForeignCode>
{
    internal interface IParent
    {
        bool ProcessErrors { get; }
        IExecutionContext ExecutionContext { get; }
        IEnumerable<Definable> DefinedNames { get; }
        Result<ValueSyntax?> ParsePredefinedItem(string source);
    }

    [DisableDump]
    internal VoidType VoidType=> this.CachedValue(() => new VoidType(this));

    [DisableDump]
    internal readonly FunctionCache<string, ForeignCodeType.Entry> ForeignLibrariesCache;

    [DisableDump]
    [Node]
    readonly FunctionList Functions = new();

    [DisableDump]
    [Node]
    readonly IParent Parent;

    readonly FunctionCache<string, FunctionCache<string, ForeignCodeType>> ForeignCodeTypeCache;
    readonly FunctionCache<Issue, IssueType> IssueTypeCache;

    [DisableDump]
    public IExecutionContext ExecutionContext => Parent.ExecutionContext;

    [DisableDump]
    [Node]
    internal BitType BitType => this.CachedValue(() => new BitType(this));

    [DisableDump]
    internal int FunctionCount => Functions.Count;

    internal static RefAlignParam DefaultRefAlignParam => new(BitsConst.SegmentAlignBits, Size.Create(64));

    [DisableDump]
    public bool? ProcessErrors => Parent.ProcessErrors;

    [DisableDump]
    internal IEnumerable<Definable> DefinedNames => Parent.DefinedNames;

    internal Root(IParent parent)
    {
        Parent = parent;
        ForeignCodeTypeCache = new(module => new(entry => new(module, entry, this)));
        IssueTypeCache = new(issue => new(this, issue));
        ForeignLibrariesCache = new(s => ForeignCodeType.CreateEntries(s, this));
    }

    IImplementation ISymbolProvider<ForeignCode>.Feature
        => this.CachedValue(() => new ForeignCodeFeature(this));

    IImplementation ISymbolProviderForPointer<ConcatArrays>.Feature
        => this.CachedValue(() => GetCreateArrayFeature(false));

    IImplementation ISymbolProviderForPointer<ForeignCode>.Feature
        => this.CachedValue(() => new ForeignCodeFeature(this));

    IImplementation ISymbolProviderForPointer<Minus>.Feature
        => this.CachedValue(GetMinusFeature);

    IImplementation ISymbolProviderForPointer<MutableConcatArrays>.Feature
        => this.CachedValue(() => GetCreateArrayFeature(true));

    [DisableDump]
    internal override Root RootContext => this;

    [DisableDump]
    internal override bool IsRecursionMode => false;

    [DisableDump]
    protected override string LevelFormat => "root context";

    internal override string ContextIdentificationDump => "r";

    internal TypeBase GetIssueType(Issue[] issues)
        => issues
            .Select(issue => IssueTypeCache[issue])
            .Aggregate((TypeBase)VoidType, (current, next) => current.GetPair(next));

    static ContextMetaFunction GetCreateArrayFeature
        (bool isMutable) => new((context, category, argsType)
        => context.CreateArrayResult(category, argsType!, isMutable)
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
        return result.Target!;
    }

    internal FunctionType GetFunctionInstance(CompoundView compoundView, FunctionSyntax body, TypeBase argsType)
    {
        var alignedArgsType = argsType.Align;
        var functionInstance = Functions.Find(body, compoundView, alignedArgsType);
        return functionInstance;
    }

    internal IEnumerable<FunctionType> GetFunctionInstances(CompoundView compoundView, FunctionSyntax body)
        => Functions.Find(body, compoundView);

    internal Result ConcatPrintResult(Category category, int count, Func<Category, int, Result> elemResults)
    {
        var result = VoidType.GetResult(category);
        if(!(category.HasCode() || category.HasClosures()))
            return result;

        StartMethodDump(false, category, count, "elemResults");
        try
        {
            if(category.HasCode())
                result.Code = "(".GetDumpPrintTextCode();

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
                        result.Code = result.Code + ", ".GetDumpPrintTextCode();
                    result.Code = result.Code + elemResult.Code;
                }

                if(category.HasClosures())
                    result.Closures = result.Closures.Sequence(elemResult.Closures);
                result.IsDirty = false;

                Dump("result", result);
                BreakExecution();
            }

            if(category.HasCode())
                result.Code = result.Code + ")".GetDumpPrintTextCode();
            return ReturnMethodDump(result);
        }
        finally
        {
            EndMethodDump();
        }
    }

    internal FunctionContainer GetFunctionContainer(int index) => Functions.Container(index);
    internal FunctionType GetFunction(int index) => Functions.Item(index);

    internal Container GetMainContainer(ValueSyntax syntax, string? description)
    {
        var rawResult = syntax.GetResultForAll(this);

        rawResult.Type.ExpectIsNotNull(() => (syntax.Anchor.SourcePart, "Type is required."));

        var result = rawResult
            .Code.GetLocalBlock(rawResult.Type.GetCopier(Category.Code).Code)
            .GetAlign();

        return new(result, rawResult.Issues.ToArray(), description);
    }

    internal TypeBase GetForeignCodeType(string module, string entry)
        => ForeignCodeTypeCache[module][entry];
}
