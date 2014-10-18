using System.Linq;
using System.Collections.Generic;
using System;
using hw.Debug;
using hw.Forms;
using hw.Helper;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Struct
{
    abstract class FunctionInstance : DumpableObject
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
            _resultCache = new ResultCache(ObtainResult);
        }

        [Node]
        [DisableDump]
        internal CodeBase BodyCode { get { return _bodyCodeCache.Value; } }
        [Node]
        string Description { get { return _body.NodeDump; } }
        [DisableDump]
        Size ArgsPartSize { get { return Parent.ArgsType.Size + RelevantValueSize; } }
        [DisableDump]
        protected abstract Size RelevantValueSize { get; }
        [Node]
        [DisableDump]
        internal CodeArgs Exts
        {
            get
            {
                var result = _resultCache.CodeArgs;
                if(result == null) // Recursive call 
                    return CodeArgs.Void(); // So, that nothing will be added from this site
                Tracer.Assert(result != null);
                return result;
            }
        }
        protected abstract FunctionId FunctionId { get; }
        [Node]
        [DisableDump]
        ContextBase Context { get { return _contextCache.Value; } }

        [DisableDump]
        internal Code.Container Container
        {
            get
            {
                try
                {
                    return BodyCode.Container(Description, FunctionId);
                }
                catch(UnexpectedVisitOfPending)
                {
                    return Code.Container.UnexpectedVisitOfPending;
                }
            }
        }

        internal Result CallResult(Category category)
        {
            var result = _resultCache & category.FunctionCall;

            if(category.HasExts)
                result.Exts = CodeArgs.Arg();
            if(category.HasCode)
                result.Code = CallType
                    .ArgCode
                    .Call(FunctionId, result.Size);
            return result;
        }

        [DisableDump]
        protected virtual TypeBase CallType { get { return Parent; } }

        Result ObtainResult(Category category)
        {
            if(IsStopByObjectIdActive)
                return null;

            var trace = FunctionId.Index < 0 && FunctionId.IsGetter && category.HasExts;
            StartMethodDump(trace, category);
            try
            {
                if(trace)
                    category = category | Category.Code;
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
                    .ReplaceAbsolute(Context.FindRecentFunctionContextObject, CreateContextRefCode, CodeArgs.Void);

                return ReturnMethodDump(result);
            }
            finally
            {
                EndMethodDump();
            }
        }

        CodeBase CreateContextRefCode()
        {
            return CodeBase
                .FrameRef(Root.DefaultRefAlignParam)
                .ReferencePlus(ArgsPartSize);
        }

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
                    .ReplaceRefsForFunctionBody(Root.DefaultRefAlignParam.RefSize, foreignRefsRef)
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

        ContextBase ObtainContext() { return Parent.CreateSubContext(!IsGetter); }
        bool IsGetter { get { return FunctionId.IsGetter; } }
    }
}