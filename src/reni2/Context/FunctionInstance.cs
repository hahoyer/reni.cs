using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using Reni.Code;
using Reni.Code.ReplaceVisitor;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Context
{
    /// <summary>
    /// Instance of a function to compile
    /// </summary>
    [Serializable]
    internal sealed class FunctionInstance : ReniObject
    {
        [Node]
        [DumpData(true)]
        private readonly TypeBase Args;

        [Node]
        [DumpData(true)]
        private readonly ICompileSyntax Body;

        [Node]
        [DumpData(true)]
        private readonly ContextBase Context;

        [DumpData(true)]
        private readonly int Index;

        [Node]
        private CodeBase _bodyCodeCache;

        /// <summary>
        /// Initializes a new instance of the FunctionInstance class.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="body">The body.</param>
        /// <param name="context">The context.</param>
        /// <param name="args">The args.</param>
        /// created 03.01.2007 21:19
        internal FunctionInstance(int index, ICompileSyntax body, ContextBase context, TypeBase args)
            : base(index)
        {
            StopByObjectId(-1);
            Index = index;
            Body = body;
            Context = context;
            Args = args;
        }

        [DumpData(false)]
        private Refs ForeignRefs
        {
            get
            {
                if(IsStopByObjectIdActive)
                    return null;
                return Result(Category.Refs).Refs;
            }
        }

        [DumpData(false)]
        internal CodeBase BodyCode
        {
            get
            {
                if(_bodyCodeCache == null)
                    _bodyCodeCache = CreateBodyCode();
                return _bodyCodeCache;
            }
        }

        [Node]
        [DumpData(false)]
        private Size FrameSize { get { return Args.Size + ForeignRefs.Size; } }

        [Node]
        [DumpData(false)]
        private string Description { get { return Body.DumpShort(); } }

        public Result CreateCall(Category category, Result args)
        {
            var trace = ObjectId == -10 && (category.HasRefs || category.HasRefs);
            StartMethodDumpWithBreak(trace, category, args);
            var localCategory = category;
            if(category.HasCode)
                localCategory = (localCategory - Category.Code) | Category.Size;
            var result = Result(localCategory).Clone();
            if(category.HasRefs)
                result.Refs = result.Refs.CreateSequence(args.Refs);

            if(category.HasCode)
                result.Code = CreateArgsAndRefForFunction(args.Code).CreateCall(Index, result.Size);

            Context.CreateFunction(Args).AssertCorrectRefs(result);
            return ReturnMethodDumpWithBreak(trace, result);
        }

        private CodeBase CreateArgsAndRefForFunction(CodeBase argsCode) { return ForeignRefs.ToCode().CreateSequence(argsCode); }

        private CodeBase CreateBodyCode()
        {
            if(IsStopByObjectIdActive)
                return null;
            var category = Category.Code;
            var refAlignParam = Context.RefAlignParam;
            var foreignRefsRef = CodeBase.CreateFrameRef(refAlignParam);
            var visitResult = Result(category);
            var result = visitResult
                .ReplaceRefsForFunctionBody(refAlignParam, foreignRefsRef);
            if(Args.Size.IsZero)
                result.Code = result.Code.TryReplacePrimitiveRecursivity(Index);
            return result.Code;
        }

        private Result Result(Category category)
        {
            if(IsStopByObjectIdActive)
                return null;

            var functionContext = Context.CreateFunction(Args);
            var trace = ObjectId == -10 && (category.HasCode || category.HasRefs);
            StartMethodDumpWithBreak(trace, category);
            var categoryEx = category | Category.Type;
            var rawResult = functionContext.Result(categoryEx, Body).Clone();

            DumpWithBreak(trace, "functionContext", functionContext, "rawResult", rawResult);

            var postProcessedResult =
                rawResult
                    .PostProcessor
                    .FunctionResult(category, functionContext.RefAlignParam);

            DumpWithBreak(trace, "postProcessedResult", postProcessedResult);
            var result =
                postProcessedResult
                    .ReplaceAbsoluteContextRef(functionContext,
                                               CreateContextRef(postProcessedResult.CompleteCategory));
            return ReturnMethodDump(trace, result);
        }

        private Result CreateContextRef(Category category)
        {
            return new Result(
                category, 
                () => Context.RefAlignParam.RefSize, 
                CreateContextRefCode, 
                Refs.None);
        }

        private CodeBase CreateContextRefCode()
        {
            var refAlignParam = Context.RefAlignParam;
            return CodeBase
                .CreateFrameRef(refAlignParam)
                .CreateRefPlus(refAlignParam,
                               FrameSize*-1);
        }

        private TypeBase Type() { return Result(Category.Type).Type; }

        internal Container Serialize(bool isInternal)
        {
            try
            {
                return BodyCode.Serialize(FrameSize, Description, isInternal);
            }
            catch(UnexpectedVisitOfPending)
            {
                return Container.UnexpectedVisitOfPending;
            }
        }

        public string DumpFunction()
        {
            var result = "\n";
            result += "index=" + Index;
            result += "\n";
            result += "body=" + Body.DumpShort();
            result += "\n";
            result += "args=" + Args.Dump();
            result += "\n";
            result += "context=" + Context.Dump();
            result += "\n";
            result += "type=" + Type().Dump();
            result += "\n";
            return result;
        }
    }

    internal sealed class ReplacePrimitiveRecursivity : Base
    {
        [DumpData(true)]
        private readonly int _functionIndex;

        public ReplacePrimitiveRecursivity(int functionIndex) { _functionIndex = functionIndex; }

        public int FunctionIndex { get { return _functionIndex; } }

        internal override CodeBase PairVisit(Code.Pair pair) { return Pair(pair, null, pair.Right.Visit(this)); }

        internal override CodeBase ChildVisit(Code.Child child) { return Child(child.Parent, child.LeafElement.Visit(this)); }

        internal override CodeBase ThenElseVisit(ThenElse This) { return ThenElse(This, null, This.ThenCode.Visit(this), This.ElseCode.Visit(this)); }

        public LeafElement CallVisit(Call This) { return This.TryConvertToRecursiveCall(_functionIndex); }
    }
}