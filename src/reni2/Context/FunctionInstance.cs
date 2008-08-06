using System;
using HWClassLibrary.Debug;
using Reni.Code;
using Reni.Code.ReplaceVisitor;
using Reni.Syntax;
using Reni.Type;
using Pair=Reni.Code.Pair;

namespace Reni.Context
{
    /// <summary>
    /// Instance of a function to compile
    /// </summary>
    [Serializable]
    internal sealed class FunctionInstance : ReniObject
    {
        private readonly TypeBase Args;
        private readonly ICompileSyntax Body;
        private readonly ContextBase Context;
        private readonly int Index;
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
                if (IsStopByObjectIdActive)
                    return null;
                return Result(Category.Refs).Refs;
            }
        }

        [DumpData(false)]
        internal CodeBase BodyCode
        {
            get
            {
                if (_bodyCodeCache == null)
                    _bodyCodeCache = CreateBodyCode();
                return _bodyCodeCache;
            }
        }

        [DumpData(false)]
        private Size FrameSize
        {
            get { return Args.Size + ForeignRefs.Size; }
        }

        [DumpData(false)]
        private string Description
        {
            get { return Body.DumpShort(); }
        }

        public Result CreateCall(Category category, Result args)
        {
            var trace = ObjectId == 3;
            StartMethodDump(trace, category, args);
            var localCategory = category;
            if (category.HasCode)
                localCategory = (localCategory - Category.Code) | Category.Size;
            var result = Result(localCategory).Clone();
            if (result.IsPending)
                return ReturnMethodDump(trace, result);


            if (category.HasRefs)
                result.Refs = result.Refs.Pair(args.Refs);

            if (trace) DumpDataWithBreak("", "result", result);

            if (category.HasCode)
            {
                var argsEx = CreateArgsAndRefForFunction(args.Code);
                result.Code = argsEx.CreateCall(Index, result.Size);
            }

            Context.CreateFunction(Args).AssertCorrectRefs(result);
            return ReturnMethodDump(trace, result);
        }

        private CodeBase CreateArgsAndRefForFunction(CodeBase argsCode)
        {
            return ForeignRefs.ToCode().CreateSequence(argsCode);
        }

        private CodeBase CreateBodyCode()
        {
            if (IsStopByObjectIdActive)
                return null;
            var category = Category.Code.Replendish();
            var refAlignParam = Context.RefAlignParam;
            var foreignRefsRef = CodeBase.CreateFrameRef(refAlignParam);
            var visitResult = Result(category);
            var result = visitResult
                .ReplaceRefsForFunctionBody(refAlignParam, foreignRefsRef);
            if (Args.Size.IsZero)
                result.Code = result.Code.TryReplacePrimitiveRecursivity(Index);
            return result.Code;
        }

        private Result Result(Category category)
        {
            if (IsStopByObjectIdActive)
                return null;

            var functionContext = Context.CreateFunction(Args);
            var trace = ObjectId == -10 && category.HasRefs;
            StartMethodDumpWithBreak(trace, category);
            var categoryEx = category;
            if (!categoryEx.IsEqual(Category.Refs))
                categoryEx = categoryEx | Category.Type;

            var result = functionContext.Result(categoryEx, Body).Clone();

            Tracer.ConditionalBreak(trace, Dump() + "\nfunctionContext=" + functionContext.Dump() + "\nresult=" + result.Dump());

            if (result.IsPending)
                return ReturnMethodDump(trace, result);

            var postProcessedResult = result.PostProcessor.FunctionResult(functionContext.AlignBits);

            Tracer.ConditionalBreak(trace, Dump() + "\npostProcessedResult=" + postProcessedResult.Dump());
            var finalResult = postProcessedResult.ReplaceAbsoluteContextRef(functionContext, CreateArgsRef(postProcessedResult.Complete));
            return ReturnMethodDump(trace, finalResult);
        }

        private Result CreateArgsRef(Category category)
        {
            var refAlignParam = Context.RefAlignParam;
            return Args.CreateAutomaticRef(refAlignParam).CreateResult(category, () => CodeBase
                                                                                  .CreateFrameRef(refAlignParam)
                                                                                  .CreateRefPlus(refAlignParam,
                                                                                                 FrameSize*-1));
        }

        private TypeBase Type()
        {
            return Result(Category.Type).Type;
        }

        internal Container Serialize(bool isInternal)
        {
            try
            {
                return BodyCode.Serialize(FrameSize, Description, isInternal);
            }
            catch (UnexpectedVisitOfPending)
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
        [DumpData(true)] private readonly int _functionIndex;

        public ReplacePrimitiveRecursivity(int functionIndex)
        {
            _functionIndex = functionIndex;
        }

        public int FunctionIndex
        {
            get { return _functionIndex; }
        }

        internal override CodeBase PairVisit(Pair pair)
        {
            return Pair(pair, null, pair.Right.Visit(this));
        }

        internal override CodeBase ChildVisit(Code.Child child)
        {
            return Child(child.Parent, child.LeafElement.Visit(this));
        }

        internal override CodeBase ThenElseVisit(Code.ThenElse This)
        {
            return ThenElse(This, null, This.ThenCode.Visit(this), This.ElseCode.Visit(this));
        }

        public LeafElement CallVisit(Call This)
        {
            return This.TryConvertToRecursiveCall(_functionIndex);
        }
    }
}