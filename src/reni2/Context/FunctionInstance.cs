using HWClassLibrary.Debug;
using Reni.Code;

namespace Reni.Context
{
    /// <summary>
    /// Instance of a function to compile
    /// </summary>
    internal sealed class FunctionInstance : ReniObject
    {
        private readonly Type.Base _args;
        private readonly Syntax.Base _body;
        private readonly Base _context;
        private readonly int _index;
        private Code.Base _bodyCodeCache;

        /// <summary>
        /// Initializes a new instance of the FunctionInstance class.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="body">The body.</param>
        /// <param name="context">The context.</param>
        /// <param name="args">The args.</param>
        /// created 03.01.2007 21:19
        internal FunctionInstance(int index, Syntax.Base body, Base context, Type.Base args)
            : base(index)
        {
            StopByObjectId(2);
            _index = index;
            _body = body;
            _context = context;
            _args = args;
        }

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
        internal Base Context { get { return _context; } }

        /// <summary>
        /// Gets the args.
        /// </summary>
        /// <value>The args.</value>
        /// created 03.01.2007 21:19
        public Type.Base Args { get { return _args; } }

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
        internal Code.Base BodyCode
        {
            get
            {
                if(_bodyCodeCache == null)
                    _bodyCodeCache = CreateBodyCode();
                return _bodyCodeCache;
            }
        }

        [DumpData(false)]
        private Size FrameSize { get { return Args.Size + ForeignRefs.Size; } }

        [DumpData(false)]
        private string Description { get { return _body.Dump(); } }

        /// <summary>
        /// Creates the call.to this function
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        /// created 03.01.2007 21:19
        public Result CreateCall(Category category, Result args)
        {
            var trace = ObjectId == -569;
            StartMethodDump(trace, category, args);
            var localCategory = category;
            if(category.HasCode)
                localCategory = (localCategory - Category.Code) | Category.Size;
            var result = Visit(localCategory).Clone();
            if(result.IsPending)
                return result;

            if(category.HasRefs)
                result.Refs = result.Refs.Pair(args.Refs);

            if(category.HasCode)
            {
                var argsEx = CreateArgsAndRefForFunction(args.Code);
                result.Code = argsEx.CreateCall(Index, result.Size);
            }
            return ReturnMethodDump(trace, result);
        }

        private Code.Base CreateArgsAndRefForFunction(Code.Base argsCode)
        {
            return ForeignRefs.ToCode().CreateSequence(argsCode);
        }

        /// <summary>
        /// Create the code of the body of this function.
        /// </summary>
        /// <returns></returns>
        /// created 31.12.2006 14:09
        private Code.Base CreateBodyCode()
        {
            if(IsStopByObjectIdActive)
                return null;
            Tracer.ConditionalBreak(_index == 2, "");
            var category = Category.Code.Replendish();
            var refAlignParam = Context.RefAlignParam;
            var foreignRefsRef = Code.Base.CreateFrameRef(refAlignParam);
            var result = Visit(category)
                .ReplaceRefsForFunctionBody(refAlignParam, foreignRefsRef);
            if(Args.Size.IsZero)
                result.Code = result.Code.TryReplacePrimitiveRecursivity(Index);
            return result.Code;
        }

        private Result Visit(Category category)
        {
            if(IsStopByObjectIdActive)
                return null;
            
            var functionContext = Context.CreateFunction(Args);
            var trace = functionContext.ObjectId == 7 && category.HasCode;
            var categoryEx = category;
            if(!categoryEx.IsEqual(Category.Refs))
                categoryEx = categoryEx | Category.Type;

            var result = Body.Visit(functionContext, categoryEx).Clone();
            
            Tracer.ConditionalBreak(trace,
                Dump() + "\nfunctionContext=" + functionContext.Dump() + "\nresult=" + result.Dump());
            
            if(result.IsPending)
                return result;

            if(result.HasType)
                result = result.Type.Dereference(result).Align(functionContext.AlignBits);

            Tracer.ConditionalBreak(trace,
                Dump() + "\nresult=" + result.Dump());
            
            result = result.ReplaceAbsoluteContextRef(functionContext, CreateArgsRef(result.Complete));
            
            Tracer.ConditionalBreak(trace, "result=" + result.Dump());

            return result;
        }

        private Result CreateArgsRef(Category category)
        {
            var refAlignParam = Context.RefAlignParam;
            return Args.CreateRef(refAlignParam).CreateResult(category, () => Code.Base
                .CreateFrameRef(refAlignParam)
                .CreateRefPlus(refAlignParam, FrameSize*-1));
        }

        private Type.Base VisitType()
        {
            return Visit(Category.Type).Type;
        }

        /// <summary>
        /// Serializes this instance.
        /// </summary>
        /// <returns></returns>
        /// created 26.11.2006 16:59
        internal Container Serialize()
        {
            try
            {
                return BodyCode.Serialize(FrameSize, Description);
            }
            catch(UnexpectedVisitOfPending)
            {
                return Container.UnexpectedVisitOfPending;
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
            result += "type=" + VisitType().Dump();
            result += "\n";
            return result;
        }
    }

    internal sealed class ReplacePrimitiveRecursivity : Code.ReplaceVisitor.Base
    {
        [DumpData(true)]
        private readonly int _functionIndex;

        public ReplacePrimitiveRecursivity(int functionIndex)
        {
            _functionIndex = functionIndex;
        }

        public int FunctionIndex { get { return _functionIndex; } }

        internal override Code.Base PairVisit(Pair pair)
        {
            return Pair(pair, null, pair.Right.Visit(this));
        }

        internal override Code.Base ChildVisit(Code.Child child)
        {
            return Child(child.Parent, child.LeafElement.Visit(this));
        }

        internal override Code.Base ThenElseVisit(Code.ThenElse This)
        {
            return ThenElse(This, null, This.ThenCode.Visit(this), This.ElseCode.Visit(this));
        }

        public LeafElement CallVisit(Call This)
        {
            return This.TryConvertToRecursiveCall(_functionIndex);
        }
    }
}