using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Helper;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Parser;
using Reni.Type;
using Reni.Validation;

namespace Reni.Struct
{
    abstract class FunctionInstance
        : DumpableObject, ResultCache.IResultProvider, ValueCache.IContainer
    {
        [Node]
        [EnableDump]
        readonly ValueSyntax Body;

        [DisableDump]
        protected readonly FunctionType Parent;

        internal readonly ResultCache ResultCache;

        bool _isObtainBodyCodeActive;

        protected FunctionInstance(FunctionType parent, ValueSyntax body)
        {
            Body = body;
            Parent = parent;
            ResultCache = new ResultCache(this);
        }

        ValueCache ValueCache.IContainer.Cache {get;} = new ValueCache();

        Result ResultCache.IResultProvider.Execute(Category category, Category pendingCategory)
        {
            if(category.IsNone && pendingCategory == Category.Closures)
                return new Result(Category.Closures, getClosures: Reni.Closures.Void);


            Tracer.Assert(pendingCategory.IsNone);
            return GetResult(category);
        }

        [Node]
        [DisableDump]
        internal CodeBase BodyCode => this.CachedValue(GetBodyCode);

        [DisableDump]
        internal CodeBase AlignedBodyCode => BodyCode?.Align();

        [DisableDump]
        Size ArgsPartSize => Parent.ArgsType.Size + RelevantValueSize;

        [DisableDump]
        protected abstract Size RelevantValueSize {get;}

        string Description => Body.Target.SourcePart.Id;

        [Node]
        [DisableDump]
        internal Closures Closures
        {
            get
            {
                var result = ResultCache.Closures;
                Tracer.Assert(result != null);
                return result;
            }
        }

        protected abstract FunctionId FunctionId {get;}

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
                    return new Container(AlignedBodyCode, Issues, Description, FunctionId);
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

        [DisableDump]
        protected virtual TypeBase CallType => Parent;

        bool IsGetter => FunctionId.IsGetter;

        [DisableDump]
        internal IEnumerable<IFormalCodeItem> CodeItems => BodyCode.Visit(new ItemCollector());

        internal Result GetCallResult(Category category)
        {
            var result = ResultCache & category.FunctionCall;
            if(result == null)
                return null;

            if(category.HasClosures)
                result.Closures = Closures.Arg();
            if(!result.HasIssue && category.HasCode)
                result.Code = CallType
                    .ArgCode
                    .Call(FunctionId, result.Size);
            return result;
        }

        Result GetResult(Category category)
        {
            if(IsStopByObjectIdActive)
                return null;

            var trace = FunctionId.Index.In() && category.HasClosures && category.HasCode;
            StartMethodDump(trace, category);
            try
            {
                Dump(nameof(Body), Body.Target.SourcePart);
                BreakExecution();
                var rawResult = Context.Result(category.WithType, Body);

                Tracer.Assert(rawResult.HasIssue || rawResult.CompleteCategory == category.WithType);
                if(rawResult.FindClosures != null)
                    Tracer.Assert(!rawResult.SmartClosures.Contains(Closures.Arg()), rawResult.Dump);

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
            if(_isObtainBodyCodeActive || IsStopByObjectIdActive)
                return null;

            try
            {
                _isObtainBodyCodeActive = true;
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
                _isObtainBodyCodeActive = false;
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
}