using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Parser;
using Reni.SyntaxTree;
using Reni.Type;
using Reni.Validation;

namespace Reni.Struct;

abstract class FunctionInstance
    : DumpableObject
        , ResultCache.IResultProvider
        , ValueCache.IContainer
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
    internal CodeBase? BodyCode => this.CachedValue(GetBodyCode);

    [DisableDump]
    internal CodeBase? AlignedBodyCode => BodyCode?.GetAlign();

    [DisableDump]
    Size ArgsPartSize => Parent.ArgumentsType.Size + RelevantValueSize;

    string Description => Body.Anchor.SourceParts.Combine()!.Id;

    [Node]
    [DisableDump]
    internal Closures Closures => ResultCache.Closures;

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

    Result ResultCache.IResultProvider.Execute(Category category) => GetResult(category);

    [DisableDump]
    protected abstract Size RelevantValueSize { get; }

    protected abstract FunctionId FunctionId { get; }

    [DisableDump]
    protected virtual TypeBase CallType => Parent;

    internal Result GetCallResult(Category category)
    {
        var result = ResultCache & category.FunctionCall();

        if(category.HasClosures())
            result.Closures = Closures.GetArgument();
        if(result.HasIssue != true && category.HasCode())
            result.Code = CallType
                .ArgumentCode
                .GetCall(FunctionId, result.Size!);
        return result;
    }

    Result GetResult(Category category)
    {
        if(IsStopByObjectIdActive)
            return null!;

        var trace = FunctionId.Index.In(-1) && category.HasCode();
        StartMethodDump(trace, category);
        try
        {
            Dump(nameof(Body), Body.Anchor.SourceParts);
            BreakExecution();
            var rawResult = Context.GetResult(category | Category.Type, Body);

            (rawResult.HasIssue || rawResult.CompleteCategory.Contains(category | Category.Type)).Assert();
            if(rawResult.FindClosures != null)
                (!rawResult.SmartClosures.Contains(Closures.GetArgument())).Assert(rawResult.Dump);

            Dump("rawResult", rawResult);
            BreakExecution();

            var adjustedResult = rawResult
                .AutomaticDereferenceResult
                .Align;

            Dump(nameof(adjustedResult), adjustedResult);
            BreakExecution();

            var postProcessedResult = adjustedResult
                .GetLocalBlock(category);

            Dump("postProcessedResult", postProcessedResult);
            BreakExecution();

            var argReferenceReplaced = ReplaceArgsReference(postProcessedResult);

            var result = argReferenceReplaced
                .ReplaceAbsolute(Context.FindRecentFunctionContextObject, CreateContextRefCode, Closures.GetVoid)
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
        if(Parent.ArgumentsType is not IContextReference reference)
            return result;

        return result
            .ReplaceAbsolute
            (
                reference
                , () => CodeBase.GetFrameRef().GetDePointer(reference.Size())
                , Closures.GetVoid
            );
    }

    CodeBase CreateContextRefCode()
        => CodeBase
            .GetFrameRef()
            .GetReferenceWithOffset(ArgsPartSize);

    CodeBase? GetBodyCode()
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
                .Code!;
            return Parent.ArgumentsType.IsHollow
                ? result.TryReplacePrimitiveRecursivity(FunctionId) 
                : result;
        }
        finally
        {
            IsObtainBodyCodeActive = false;
        }
    }

    public string DumpFunction()
    {
        var result = "\n";
        result += "body=" + Body!.NodeDump;
        result += "\n";
        return result;
    }

    ContextBase GetContext() => Parent.CreateSubContext(!IsGetter);
}