using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.TreeStructure;
using Reni.Basics;
using Reni.Code;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Struct
{
    /// <summary>
    ///     Instance of a function to compile
    /// </summary>
    [Serializable]
    internal sealed class FunctionInstance : ReniObject
    {
        [Node]
        [EnableDump]
        private readonly TypeBase _args;

        [Node]
        [EnableDump]
        private readonly ICompileSyntax _body;

        [Node]
        [EnableDump]
        private readonly Structure _structure;

        [EnableDump]
        private readonly int _index;

        [Node]
        private readonly SimpleCache<CodeBase> _bodyCodeCache;

        /// <summary>
        ///     Initializes a new instance of the FunctionInstance class.
        /// </summary>
        /// <param name = "index">The index.</param>
        /// <param name = "body">The body.</param>
        /// <param name = "structure">The context.</param>
        /// <param name = "args">The args.</param>
        /// created 03.01.2007 21:19
        internal FunctionInstance(int index, ICompileSyntax body, Structure structure, TypeBase args)
            : base(index)
        {
            _index = index;
            _body = body;
            _structure = structure;
            _args = args;
            _bodyCodeCache = new SimpleCache<CodeBase>(CreateBodyCode);
            StopByObjectId(-10);
        }

        [DisableDump]
        private Refs ForeignRefs
        {
            get
            {
                if(IsStopByObjectIdActive)
                    return null;
                return Result(Category.Refs).Refs;
            }
        }

        [DisableDump]
        internal CodeBase BodyCode { get { return _bodyCodeCache.Value; } }

        internal void EnsureBodyCode() { _bodyCodeCache.Ensure(); }

        [Node]
        [DisableDump]
        private Size FrameSize { get { return _args.Size + ForeignRefs.Size; } }

        [Node]
        [DisableDump]
        private string Description { get { return _body.DumpShort(); } }

        public Result CreateCall(Category category, Result args)
        {
            var trace = ObjectId == -10 && (category.HasRefs || category.HasRefs);
            StartMethodDumpWithBreak(trace, category, args);
            var localCategory = category;
            if(category.HasCode)
                localCategory = (localCategory - Category.Code) | Category.Size;
            var result = Result(localCategory).Clone();
            if(category.HasRefs)
                result.Refs = result.Refs.Sequence(args.Refs);

            if(category.HasCode)
                result.Code = CreateArgsAndRefForFunction(args.Code).Call(_index, result.Size);

            return ReturnMethodDumpWithBreak(trace, result);
        }

        private CodeBase CreateArgsAndRefForFunction(CodeBase argsCode) { return ForeignRefs.ToCode().Sequence(argsCode); }

        private CodeBase CreateBodyCode()
        {
            if(IsStopByObjectIdActive)
                return null;
            var category = Category.Code;
            var refAlignParam = _structure.UniqueContext.RefAlignParam;
            var foreignRefsRef = CodeBase.FrameRef(refAlignParam);
            var visitResult = Result(category);
            var result = visitResult
                .ReplaceRefsForFunctionBody(refAlignParam, foreignRefsRef);
            if(_args.Size.IsZero)
                result.Code = result.Code.TryReplacePrimitiveRecursivity(_index);
            return result.Code;
        }

        private Result Result(Category category)
        {
            if(IsStopByObjectIdActive)
                return null;

            var functionContext = _structure.UniqueContext.UniqueChildContext(_args);
            var trace = ObjectId == -10 && (category.HasCode || category.HasRefs);
            StartMethodDumpWithBreak(trace, category);
            var categoryEx = category | Category.Type;
            var rawResult = functionContext.Result(categoryEx, _body).Clone();

            DumpWithBreak(trace, "functionContext", functionContext, "rawResult", rawResult);

            var postProcessedResult =
                rawResult
                    .PostProcessor
                    .FunctionResult(category, _structure.RefAlignParam);

            DumpWithBreak(trace, "postProcessedResult", postProcessedResult);
            var result =
                postProcessedResult
                    .ReplaceAbsolute(functionContext.FindRecentFunctionContextObject, CreateContextRefCode, Refs.None);
            return ReturnMethodDump(trace, result);
        }

        private CodeBase CreateContextRefCode()
        {
            var refAlignParam = _structure.UniqueContext.RefAlignParam;
            return CodeBase
                .FrameRef(refAlignParam)
                .AddToReference(refAlignParam, FrameSize*-1);
        }

        private TypeBase Type() { return Result(Category.Type).Type; }

        internal Code.Container Serialize(bool isInternal)
        {
            try
            {
                return new Code.Container(BodyCode, FrameSize, Description, isInternal);
            }
            catch(UnexpectedVisitOfPending)
            {
                return Code.Container.UnexpectedVisitOfPending;
            }
        }

        public string DumpFunction()
        {
            var result = "\n";
            result += "index=" + _index;
            result += "\n";
            result += "body=" + _body.DumpShort();
            result += "\n";
            result += "args=" + _args.Dump();
            result += "\n";
            result += "context=" + _structure.Dump();
            result += "\n";
            result += "type=" + Type().Dump();
            result += "\n";
            return result;
        }
    }
}