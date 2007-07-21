using System;
using HWClassLibrary.Debug;
using Reni.Code;

namespace Reni.Context
{
    /// <summary>
    /// Instance of a function to compile
    /// </summary>
    public sealed class FunctionInstance: ReniObject
    {
        readonly int _index;
        readonly Syntax.Base _body;
        readonly Base _context;
        readonly Type.Base _args;
        private Code.Base _bodyCodeCache;

        /// <summary>
        /// Gets the index.that is unique for each function
        /// </summary>
        /// <value>The index.</value>
        /// created 03.01.2007 21:18
        public int Index { get { return _index; } }
        /// <summary>
        /// Gets the body.
        /// </summary>
        /// <value>The body.</value>
        /// created 03.01.2007 21:18
        public Syntax.Base Body { get { return _body; } }
        /// <summary>
        /// Gets the context.the function is defined in
        /// </summary>
        /// <value>The context.</value>
        /// created 03.01.2007 21:18
        public Base Context { get { return _context; } }
        /// <summary>
        /// Gets the args.
        /// </summary>
        /// <value>The args.</value>
        /// created 03.01.2007 21:19
        public Type.Base Args { get { return _args; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:FunctionInstance"/> class.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="body">The body.</param>
        /// <param name="context">The context.</param>
        /// <param name="args">The args.</param>
        /// created 03.01.2007 21:19
        public FunctionInstance(int index, Syntax.Base body, Base context, Type.Base args)
            : base(index)
        {
            _index = index;
            _body = body;
            _context = context;
            _args = args;
        }

        /// <summary>
        /// Creates the call.to this function
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        /// created 03.01.2007 21:19
        public Result CreateCall(Category category, Result args)
        {

            bool trace = ObjectId == -569;
            StartMethodDump(trace, category, args);
            Category localCategory = category;
            if (category.HasCode)
                localCategory = (localCategory - Category.Code) | Category.Size;
            Result result = Visit(localCategory).Clone();
            if (result.IsPending)
                return result;

            if (category.HasRefs)
                result.Refs = result.Refs.Pair(args.Refs);

            if (category.HasCode)
            {
                Code.Base argsEx = CreateArgsAndRefForFunction(args.Code);
                result.Code = argsEx.CreateCall(Index, result.Size);
            }
            return ReturnMethodDump(trace, result);
        }

        private Code.Base CreateArgsAndRefForFunction(Code.Base argsCode)
        {
            return ForeignRefs.ToCode().CreateSequence(argsCode);
        }

        [DumpData(false)]
        private Refs ForeignRefs
        {
            get
            {
                if(IsStopByObjectIdActive)
                    return null;
                return Visit(Category.Refs).Refs;
            }
        }

        [DumpData(false)]
        public Code.Base BodyCode
        {
             get
             {
                 if (_bodyCodeCache == null)
                     _bodyCodeCache = CreateBodyCode();
                 return _bodyCodeCache;
             }
        }
        /// <summary>
        /// Create the code of the body of this function.
        /// </summary>
        /// <returns></returns>
        /// created 31.12.2006 14:09
        Code.Base CreateBodyCode()
        {
            if (IsStopByObjectIdActive)
                return null;
            Category category = Category.Code.Replendish();
            RefAlignParam refAlignParam = Context.RefAlignParam;
            Code.Base foreignRefsRef = Code.Base.CreateFrameRef(refAlignParam);
            Result result = Visit(category)
                .ReplaceRefsForFunctionBody(refAlignParam,foreignRefsRef);
            if (Args.Size.IsZero)
                result.Code = result.Code.TryReplacePrimitiveRecursivity(Index);
            return result.Code;
        }

        private Result Visit(Category category)
        {
            if(IsStopByObjectIdActive)
                return null;
            Function functionContext = Context.CreateFunction(Args);
            bool trace = functionContext.ObjectId == 10 && category.HasCode;
            Category categoryEx = category;
            if (!categoryEx.IsEqual(Category.Refs))
                categoryEx = categoryEx | Category.Type;

            Result result = Body.Visit(functionContext, categoryEx).Clone();
            if (result.IsPending)
                return result;
            if (categoryEx.HasType)
                result = result.Type.Dereference(result);
            if (category.HasCode)
            {
                RefAlignParam refAlignParam = Context.RefAlignParam;
                Code.Base argsRef =
                    Code.Base
                    .CreateFrameRef(refAlignParam)
                    .CreateRefPlus(refAlignParam, FrameSize * -1);
                Tracer.ConditionalBreak(trace, Dump() + "\nfunctionContext=" + functionContext.Dump() + "\nresult=" + result.Dump());
                result = result.ReplaceAbsoluteContextRef(functionContext, argsRef);
                Tracer.ConditionalBreak(trace, "result=" + result.Dump());
            }
            else if (category.HasRefs)
                result.Refs = result.Refs.Without(functionContext);

            return result;
        }

        /// <summary>
        /// Serializes this instance.
        /// </summary>
        /// <returns></returns>
        /// created 26.11.2006 16:59
        public Container Serialize()
        {
            try
            {
                return BodyCode.Serialize(FrameSize, Description);
            }
            catch (UnexpectedVisitOfPending)
            {
                return Container.UnexpectedVisitOfPending;
            }
        }

        [DumpData(false)]
        private Size FrameSize { get { return Args.Size + ForeignRefs.Size; } }

        [DumpData(false)]
        private string Description { get { return _body.Dump(); } }

        public string DumpFunction()
        {
            string result = "\n";
            result += "index=" + _index;
            result += "\n";
            result += "body=" + _body.DumpShort();
            result += "\n";
            result += "args=" + _args.Dump();
            result += "\n";
            result += "context=" + _context.Dump();
            result += "\n";
            return result;
        }
    }

    public class ReplacePrimitiveRecursivity : Code.ReplaceVisitor.Base
    {
        [DumpData(true)]
        private readonly int _functionIndex;

        public ReplacePrimitiveRecursivity(int functionIndex)
        {
            _functionIndex = functionIndex;
        }

        public int FunctionIndex { get { return _functionIndex; } }

        public override Code.Base PairVisit(Pair pair)
        {
            return Pair(pair, null, pair.Right.Visit(this));
        }

        public override Code.Base ChildVisit(Code.Child child)
        {
            return Child(child.Parent,child.LeafElement.Visit(this));
        }

        public override Code.Base ThenElseVisit(Code.ThenElse This)
        {
            return ThenElse(This,null,This.ThenCode.Visit(this),This.ElseCode.Visit(this));
        }

        public LeafElement CallVisit(Call This)
        {
            return This.TryConvertToRecursiveCall(_functionIndex);
        }
    }
}