using HWClassLibrary.Debug;
using Reni.Context;

namespace Reni.Code
{
    public class Call : LeafElement
    {
        private readonly int _functionIndex;
        private readonly Size _resultSize;
        private readonly Size _argsAndRefsSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="Call"/> class.
        /// </summary>
        /// <param name="functionIndex">Index of the function.</param>
        /// <param name="resultSize">Size of the result.</param>
        /// <param name="argsAndRefsSize">Size of the args.</param>
        /// created 12.11.2006 14:12
        public Call(int functionIndex, Size resultSize, Size argsAndRefsSize)
        {
            _functionIndex = functionIndex;
            _resultSize = resultSize;
            _argsAndRefsSize = argsAndRefsSize;
        }
		/// <summary>
		/// Index to identify function, also index in function table
		/// </summary>
		public int FunctionIndex { get { return _functionIndex; } }
        /// <summary>
        /// Size of function result
        /// </summary>
	    public Size ResultSize { get { return _resultSize; } }

        /// <summary>
        /// Size of object
        /// </summary>
        public override Size Size { get { return ResultSize; } }

        /// <summary>
        /// Gets the size of the args.
        /// </summary>
        /// <value>The size of the args.</value>
        /// created 12.11.2006 14:12
        public Size ArgsAndRefsSize { get { return _argsAndRefsSize; } }

        /// <summary>
        /// Gets the size of the delta.
        /// </summary>
        /// <value>The size of the delta.</value>
        /// created 10.10.2006 00:21
        public override Size DeltaSize { get { return ArgsAndRefsSize - ResultSize; } }

        /// <summary>
        /// Formats this instance.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <returns></returns>
        /// created 07.10.2006 21:11
        protected override string Format(StorageDescriptor start)
        {
            return start.Call(FunctionIndex,ArgsAndRefsSize);
        }

        internal override LeafElement Visit(ReplacePrimitiveRecursivity replacePrimitiveRecursivity)
        {
            return replacePrimitiveRecursivity.CallVisit(this);
        }

        public LeafElement TryConvertToRecursiveCall(int functionIndex)
        {
            if (FunctionIndex != functionIndex)
                return this;
            Tracer.Assert(ResultSize.IsZero);
            return Base.CreateRecursiveCall(ArgsAndRefsSize);
        }

    }

    /// <summary>
    /// Code element for a call that has been resolved as simple recursive call candidate. 
    /// This implies, that the call is contained in the function called. 
    /// It must not have any argument and should return nothing. 
    /// It will be assembled as a jump to begin of function.
    /// </summary>
    internal class RecursiveCallCandidate : LeafElement
    {
        private readonly Size _refsSize;

        public Size RefsSize { get { return _refsSize; } }

        /// <summary>
        /// Gets the size.
        /// </summary>
        /// <value>The size.</value>
        /// created 05.10.2006 23:40
        public override Size Size { get { return Size.Zero; } }

        /// <summary>
        /// Tries to combine back.
        /// </summary>
        /// <param name="precedingElement">The preceding element.</param>
        /// <returns></returns>
        /// created 04.01.2007 15:07
        internal override LeafElement TryToCombineBack(TopFrame precedingElement)
        {
            if ((DeltaSize + precedingElement.DeltaSize).IsZero 
                && (precedingElement.Offset + RefsSize).IsZero)
                return new RecursiveCall();
            return base.TryToCombineBack(precedingElement);
        }

        /// <summary>
        /// Gets the size of the delta.
        /// </summary>
        /// <value>The size of the delta.</value>
        /// created 10.10.2006 00:21
        public override Size DeltaSize { get { return RefsSize; } }

        public RecursiveCallCandidate(Size refsSize)
        {
            _refsSize = refsSize;
        }

    }

    internal class RecursiveCall : LeafElement
    {
        /// <summary>
        /// Gets the size.
        /// </summary>
        /// <value>The size.</value>
        /// created 05.10.2006 23:40
        public override Size Size { get { return Size.Zero; } }

        /// <summary>
        /// Formats this instance.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <returns></returns>
        /// created 07.10.2006 21:11
        protected override string Format(StorageDescriptor start)
        {
            return start.RecursiveCall();
        }

        /// <summary>
        /// Gets the size of the delta.
        /// </summary>
        /// <value>The size of the delta.</value>
        /// created 10.10.2006 00:21
        public override Size DeltaSize { get { return Size.Zero; } }
    }
}
