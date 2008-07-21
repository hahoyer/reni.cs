using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Context;

namespace Reni.Code
{
    sealed internal class Call : LeafElement
    {
        [Node]
        private readonly int FunctionIndex;
        [Node]
        private readonly Size ResultSize;
        [Node]
        private readonly Size ArgsAndRefsSize;

        internal Call(int functionIndex, Size resultSize, Size argsAndRefsSize)
        {
            FunctionIndex = functionIndex;
            ResultSize = resultSize;
            ArgsAndRefsSize = argsAndRefsSize;
        }

        protected override Size GetSize()
        {
            return ResultSize;
        }

        protected override Size GetDeltaSize()
        {
            return ArgsAndRefsSize - ResultSize;
        }

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
            return CodeBase.CreateRecursiveCall(ArgsAndRefsSize);
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
        [Node]
        private readonly Size RefsSize;

        protected override Size GetSize()
        {
            return Size.Zero;
        }

        protected override Size GetDeltaSize()
        {
            return RefsSize;
        }

        internal override LeafElement TryToCombineBack(TopFrame precedingElement)
        {
            if ((GetDeltaSize() + precedingElement.DeltaSize).IsZero 
                && (precedingElement.Offset + RefsSize).IsZero)
                return new RecursiveCall();
            return base.TryToCombineBack(precedingElement);
        }

        public RecursiveCallCandidate(Size refsSize)
        {
            RefsSize = refsSize;
        }

    }

    internal class RecursiveCall : LeafElement
    {
        protected override Size GetSize()
        {
            return Size.Zero;
        }

        protected override Size GetDeltaSize()
        {
            return Size.Zero;
        }

        protected override string Format(StorageDescriptor start)
        {
            return start.RecursiveCall();
        }
    }
}
