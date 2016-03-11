using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Forms;
using hw.Helper;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Parser;
using Reni.Type;

namespace Reni.Struct
{
    abstract class FunctionInstance
        : DumpableObject
            , ResultCache.IResultProvider
            , ValueCache.IContainer
    {
        [DisableDump]
        protected readonly FunctionType Parent;
        [Node]
        [EnableDump]
        readonly CompileSyntax Body;

        internal readonly ResultCache ResultCache;

        protected FunctionInstance(FunctionType parent, CompileSyntax body)
        {
            Body = body;
            Parent = parent;
            ResultCache = new ResultCache(this);
        }

        ValueCache ValueCache.IContainer.Cache { get; } = new ValueCache();

        [Node]
        [DisableDump]
        internal CodeBase BodyCode => ValueCacheExtension.CachedValue(this, GetBodyCode);
        [DisableDump]
        Size ArgsPartSize => Parent.ArgsType.Size + RelevantValueSize;
        [DisableDump]
        protected abstract Size RelevantValueSize { get; }

        string Description => Body.SourcePart.Id;

        [Node]
        [DisableDump]
        internal CodeArgs Exts
        {
            get
            {
                var result = ResultCache.Exts;
                Tracer.Assert(result != null);
                return result;
            }
        }

        protected abstract FunctionId FunctionId { get; }
        [Node]
        [DisableDump]
        ContextBase Context => ValueCacheExtension.CachedValue(this, GetContext);

        [DisableDump]
        internal Container Container
        {
            get
            {
                try
                {
                    return BodyCode.Container(Description, FunctionId);
                }
                catch(UnexpectedVisitOfPending)
                {
                    return Container.UnexpectedVisitOfPending;
                }
            }
        }

        internal Result GetCallResult(Category category)
        {
            var result = ResultCache & category.FunctionCall;
            if(result == null)
                return null;

            if(category.HasExts)
                result.Exts = CodeArgs.Arg();
            if(category.HasCode)
                result.Code = CallType
                    .ArgCode
                    .Call(FunctionId, result.Size);
            return result;
        }

        [DisableDump]
        protected virtual TypeBase CallType => Parent;

        Result GetResult(Category category)
        {
            if(IsStopByObjectIdActive)
                return null;

            var trace = FunctionId.Index.In() && category.HasCode;
            StartMethodDump(trace, category);
            try
            {
                Dump(nameof(Body), Body.SourcePart);
                BreakExecution();
                var rawResult = Context.Result(category.Typed, Body);

                Tracer.Assert(rawResult.CompleteCategory == category.Typed);
                if(rawResult.FindExts != null)
                    Tracer.Assert(!rawResult.SmartExts.Contains(CodeArgs.Arg()), rawResult.Dump);

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
                    (Context.FindRecentFunctionContextObject, CreateContextRefCode, CodeArgs.Void)
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
                (reference, () => CodeBase.FrameRef().DePointer(reference.Size()), CodeArgs.Void);
        }

        CodeBase CreateContextRefCode()
            => CodeBase
                .FrameRef()
                .ReferencePlus(ArgsPartSize);

        bool _isObtainBodyCodeActive;

        CodeBase GetBodyCode()
        {
            if(_isObtainBodyCodeActive || IsStopByObjectIdActive)
                return null;

            try
            {
                _isObtainBodyCodeActive = true;
                var foreignRefsRef = CreateContextRefCode();
                var visitResult = ResultCache & (Category.Code | Category.Exts);
                var result = visitResult
                    .ReplaceRefsForFunctionBody(foreignRefsRef)
                    .Code;
                if(Parent.ArgsType.Hllw)
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
        bool IsGetter => FunctionId.IsGetter;

        Result ResultCache.IResultProvider.Execute(Category category, Category pendingCategory)
        {
            if(category.IsNone && pendingCategory == Category.Exts)
                return new Result(Category.Exts, getExts: CodeArgs.Void);


            Tracer.Assert(pendingCategory.IsNone);
            return GetResult(category);
        }

        ResultCache.IResultProvider ResultCache.IResultProvider.FindSource(IContextReference ext)
        {
            NotImplementedMethod(ext);
            return null;
        }

        [DisableDump]
        internal IEnumerable<IFormalCodeItem> CodeItems => BodyCode.Visit(new ItemCollector());
    }
}