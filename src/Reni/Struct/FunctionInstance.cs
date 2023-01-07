using hw.DebugFormatter;
using hw.Helper;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Parser;
using Reni.SyntaxTree;
using Reni.Type;
using Reni.Validation;

namespace Reni.Struct;

abstract class FunctionInstance
    : DumpableObject, ResultCache.IResultProvider, ValueCache.IContainer
{
    [DisableDump]
    protected readonly FunctionType Parent;

    internal readonly ResultCache ResultCache;

    [Node]
    [EnableDump]
    readonly ValueSyntax Body;

    bool IsObtainBodyCodeActive;

    [Node]
    [DisableDump]
    internal CodeBase BodyCode => this.CachedValue(GetBodyCode);

    [DisableDump]
    internal CodeBase AlignedBodyCode => BodyCode?.Align();

    [DisableDump]
    Size ArgsPartSize => Parent.ArgsType.Size + RelevantValueSize;

    string Description => Body.Anchor.SourceParts.Combine().Id ?? "";

    [Node]
    [DisableDump]
    internal Closures Closures
    {
        get
        {
            var result = ResultCache.Closures;
            (result != null).Assert();
            return result;
        }
    }

    [Node]
    [DisableDump]
    ContextBase Context => this.CachedValue(GetContext);

    [DisableDump]
    internal Container Container
    {
        get
        {
            try
            {
                return new(AlignedBodyCode, Issues, Description, FunctionId);
            }
            catch(UnexpectedVisitOfPending)
            {
                return Container.UnexpectedVisitOfPending;
            }
        }
    }

    [Node]
    [DisableDump]
    internal Issue[] Issues => ResultCache.Issues;

    bool IsGetter => FunctionId.IsGetter;

    [DisableDump]
    internal IEnumerable<IFormalCodeItem> CodeItems => BodyCode.Visit(new ItemCollector());

    protected FunctionInstance(FunctionType parent, ValueSyntax body)
    {
        Body = body;
        Parent = parent;
        ResultCache = new(this);
    }

    ValueCache ValueCache.IContainer.Cache { get; } = new();

    Result ResultCache.IResultProvider.Execute(Category category, Category pendingCategory)
    {
        if(category == Category.None && pendingCategory == Category.Closures)
            return new(Category.Closures, Closures.Void);

        (pendingCategory == Category.None).Assert();
        return GetResult(category);
    }

    [DisableDump]
    protected abstract Size RelevantValueSize { get; }

    protected abstract FunctionId FunctionId { get; }

    [DisableDump]
    protected virtual TypeBase CallType => Parent;

    internal Result GetCallResult(Category category)
    {
        var result = ResultCache & category.FunctionCall();
        if(result == null)
            return null;

        if(category.HasClosures())
            result.Closures = Closures.Arg();
        if(result.HasIssue != true && category.HasCode())
            result.Code = CallType
                .ArgCode
                .Call(FunctionId, result.Size);
        return result;
    }

    Result GetResult(Category category)
    {
        if(IsStopByObjectIdActive)
            return null;

        var trace = FunctionId.Index.In() && category.HasClosures() && category.HasCode();
        StartMethodDump(trace, category);
        try
        {
            Dump(nameof(Body), Body.Anchor.SourceParts);
            BreakExecution();
            var rawResult = Context.Result(category | Category.Type, Body);

            (rawResult.HasIssue || rawResult.CompleteCategory == (category | Category.Type)).Assert();
            if(rawResult.FindClosures != null)
                (!rawResult.SmartClosures.Contains(Closures.Arg())).Assert(rawResult.Dump);

            Dump("rawResult", rawResult);
            BreakExecution();

            var adjustedResult = rawResult
                .AutomaticDereferenceResult
                .Align;

            Dump(nameof(adjustedResult), adjustedResult);
            BreakExecution();

            var postProcessedResult = adjustedResult
                .LocalBlock(category);

            Dump("postProcessedResult", postProcessedResult);
            BreakExecution();

            var argReferenceReplaced = ReplaceArgsReference(postProcessedResult);

            var result = argReferenceReplaced
                .ReplaceAbsolute
                    (Context.FindRecentFunctionContextObject, CreateContextRefCode, Closures.Void)
                .Weaken;

            return ReturnMethodDump(result);
        }
        finally
        {
            EndMethodDump();
        }
    }

    Result ReplaceArgsReference(Result result)
    {
        var reference = Parent.ArgsType as IContextReference;
        if(reference == null)
            return result;

        return result
            .ReplaceAbsolute
                (reference, () => CodeBase.FrameRef().DePointer(reference.Size()), Closures.Void);
    }

    CodeBase CreateContextRefCode()
        => CodeBase
            .FrameRef()
            .ReferencePlus(ArgsPartSize);

    CodeBase GetBodyCode()
    {
        if(IsObtainBodyCodeActive || IsStopByObjectIdActive)
            return null;

        try
        {
            IsObtainBodyCodeActive = true;
            var foreignRefsRef = CreateContextRefCode();
            var visitResult = ResultCache & (Category.Code | Category.Closures);
            var result = visitResult
                .ReplaceRefsForFunctionBody(foreignRefsRef)
                .Code;
            if(Parent.ArgsType.IsHollow)
                return result.TryReplacePrimitiveRecursivity(FunctionId);
            return result;
        }
        finally
        {
            IsObtainBodyCodeActive = false;
        }
    }

    public string DumpFunction()
    {
        var result = "\n";
        result += "body=" + Body.NodeDump;
        result += "\n";
        return result;
    }

    ContextBase GetContext() => Parent.CreateSubContext(!IsGetter);
}