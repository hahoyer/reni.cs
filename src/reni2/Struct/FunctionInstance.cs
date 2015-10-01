using System.Linq;
using System.Collections.Generic;
using System;
using hw.Debug;
using hw.Forms;
using hw.Helper;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Parser;
using Reni.Type;

namespace Reni.Struct
{
    abstract class FunctionInstance : DumpableObject, ResultCache.IResultProvider
    {
        [DisableDump]
        protected readonly FunctionType Parent;
        [Node]
        [EnableDump]
        readonly CompileSyntax _body;

        readonly ValueCache<CodeBase> _bodyCodeCache;
        readonly ValueCache<ContextBase> _contextCache;
        readonly ResultCache _resultCache;

        protected FunctionInstance(FunctionType parent, CompileSyntax body)
        {
            _body = body;
            Parent = parent;
            _bodyCodeCache = new ValueCache<CodeBase>(ObtainBodyCode);
            _contextCache = new ValueCache<ContextBase>(ObtainContext);
            _resultCache = new ResultCache(this);
        }

        [Node]
        [DisableDump]
        internal CodeBase BodyCode => _bodyCodeCache.Value;
        [DisableDump]
        Size ArgsPartSize => Parent.ArgsType.Size + RelevantValueSize;
        [DisableDump]
        protected abstract Size RelevantValueSize { get; }
        [Node]
        [DisableDump]
        internal CodeArgs Exts
        {
            get
            {
                var result = _resultCache.Exts;
                Tracer.Assert(result != null);
                return result;
            }
        }
        protected abstract FunctionId FunctionId { get; }
        [Node]
        [DisableDump]
        ContextBase Context => _contextCache.Value;

        [DisableDump]
        internal Container Container
        {
            get
            {
                try
                {
                    return BodyCode.Container("Description", FunctionId);
                }
                catch(UnexpectedVisitOfPending)
                {
                    return Container.UnexpectedVisitOfPending;
                }
            }
        }

        internal Result CallResult(Category category)
        {
            var result = _resultCache & category.FunctionCall;      
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

        Result ObtainResult(Category category)
        {
            if(IsStopByObjectIdActive)
                return null;

            var trace = FunctionId.Index == -2 && FunctionId.IsGetter && category.HasType;
            StartMethodDump(trace, category);
            try
            {
                BreakExecution();
                var rawResult = Context.Result(category.Typed, _body);

                Tracer.Assert(rawResult.CompleteCategory == category.Typed);
                if(rawResult.FindArgs != null)
                    Tracer.Assert(!rawResult.SmartArgs.Contains(CodeArgs.Arg()), rawResult.Dump);

                Dump("rawResult", rawResult);
                BreakExecution();

                var automaticDereferenceResult = rawResult.AutomaticDereferenceResult;

                Dump("automaticDereferenceResult", automaticDereferenceResult);
                BreakExecution();

                var postProcessedResult = automaticDereferenceResult.Align
                    .LocalBlock(category);

                Dump("postProcessedResult", postProcessedResult);
                BreakExecution();

                var result = postProcessedResult
                    .ReplaceAbsolute
                    (Context.FindRecentFunctionContextObject, CreateContextRefCode, CodeArgs.Void);

                return ReturnMethodDump(result);
            }
            finally
            {
                EndMethodDump();
            }
        }

        CodeBase CreateContextRefCode()
            => CodeBase
                .FrameRef()
                .ReferencePlus(ArgsPartSize);

        bool _isObtainBodyCodeActive;

        CodeBase ObtainBodyCode()
        {
            if(_isObtainBodyCodeActive || IsStopByObjectIdActive)
                return null;

            try
            {
                _isObtainBodyCodeActive = true;
                var foreignRefsRef = CreateContextRefCode();
                var visitResult = _resultCache & (Category.Code | Category.Exts);
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
            result += "body=" + _body.NodeDump;
            result += "\n";
            return result;
        }

        ContextBase ObtainContext() => Parent.CreateSubContext(!IsGetter);
        bool IsGetter => FunctionId.IsGetter;

        Result ResultCache.IResultProvider.Execute(Category category, Category pendingCategory)
        {
            if(category.IsNone && pendingCategory == Category.Exts)
                return new Result(Category.Exts, getExts: CodeArgs.Void);


            Tracer.Assert(pendingCategory.IsNone);
            return ObtainResult(category);
        }

        object ResultCache.IResultProvider.Target => this;
    }
}