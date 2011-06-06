using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.TreeStructure;
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
        [IsDumpEnabled(true)]
        private readonly TypeBase _args;

        [Node]
        [IsDumpEnabled(true)]
        private readonly ICompileSyntax _body;

        [Node]
        [IsDumpEnabled(true)]
        private readonly Struct.Context _context;

        [IsDumpEnabled(true)]
        private readonly int _index;

        [Node]
        private readonly SimpleCache<CodeBase> _bodyCodeCache;

        /// <summary>
        ///     Initializes a new instance of the FunctionInstance class.
        /// </summary>
        /// <param name = "index">The index.</param>
        /// <param name = "body">The body.</param>
        /// <param name = "context">The context.</param>
        /// <param name = "args">The args.</param>
        /// created 03.01.2007 21:19
        internal FunctionInstance(int index, ICompileSyntax body, Struct.Context context, TypeBase args)
            : base(index)
        {
            StopByObjectId(-1);
            _index = index;
            _body = body;
            _context = context;
            _args = args;
            _bodyCodeCache = new SimpleCache<CodeBase>(CreateBodyCode);
        }

        [IsDumpEnabled(false)]
        private Refs ForeignRefs
        {
            get
            {
                if(IsStopByObjectIdActive)
                    return null;
                return Result(Category.Refs).Refs;
            }
        }

        [IsDumpEnabled(false)]
        internal CodeBase BodyCode { get { return _bodyCodeCache.Value; } }

        internal void EnsureBodyCode() { _bodyCodeCache.Ensure(); }

        [Node]
        [IsDumpEnabled(false)]
        private Size FrameSize { get { return _args.Size + ForeignRefs.Size; } }

        [Node]
        [IsDumpEnabled(false)]
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
                result.Code = CreateArgsAndRefForFunction(args.Code).CreateCall(_index, result.Size);

            _context.ToContext.CreateFunction(_args).AssertCorrectRefs(result);
            return ReturnMethodDumpWithBreak(trace, result);
        }

        private CodeBase CreateArgsAndRefForFunction(CodeBase argsCode) { return ForeignRefs.ToCode().Sequence(argsCode); }

        private CodeBase CreateBodyCode()
        {
            if(IsStopByObjectIdActive)
                return null;
            var category = Category.Code;
            var refAlignParam = _context.ToContext.RefAlignParam;
            var foreignRefsRef = CodeBase.FrameRef(refAlignParam, "FunctionInstance.CreateBodyCode");
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

            var functionContext = _context.ToContext.CreateFunction(_args);
            var trace = ObjectId == -10 && (category.HasCode || category.HasRefs);
            StartMethodDumpWithBreak(trace, category);
            var categoryEx = category | Category.Type;
            var rawResult = functionContext.Result(categoryEx, _body).Clone();

            DumpWithBreak(trace, "functionContext", functionContext, "rawResult", rawResult);

            var postProcessedResult =
                rawResult
                    .PostProcessor
                    .FunctionResult(category, functionContext.RefAlignParam);

            DumpWithBreak(trace, "postProcessedResult", postProcessedResult);
            var result =
                postProcessedResult
                    .ReplaceAbsolute(functionContext,
                                     () => CreateContextRef(postProcessedResult.CompleteCategory));
            return ReturnMethodDump(trace, result);
        }

        private Result CreateContextRef(Category category)
        {
            return new Result(
                category,
                () => _context.ToContext.RefAlignParam.RefSize,
                CreateContextRefCode,
                Refs.None);
        }

        private CodeBase CreateContextRefCode()
        {
            var refAlignParam = _context.ToContext.RefAlignParam;
            return CodeBase
                .FrameRef(refAlignParam, "FunctionInstance.CreateContextRefCode")
                .AddToReference(refAlignParam, FrameSize*-1, "FunctionInstance.CreateContextRefCode");
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
            result += "context=" + _context.Dump();
            result += "\n";
            result += "type=" + Type().Dump();
            result += "\n";
            return result;
        }
    }
}