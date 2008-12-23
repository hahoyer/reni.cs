using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Context;

namespace Reni.Code
{
    [Serializable]

    sealed internal class Call : LeafElement
    {
        [Node]
        internal readonly int FunctionIndex;
        [Node]
        private readonly Size ResultSize;
        [Node]
        internal readonly Size ArgsAndRefsSize;

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

        protected override Size GetInputSize()
        {
            return ArgsAndRefsSize;
        }

        protected override string Format(StorageDescriptor start)
        {
            return start.CreateCall(FunctionIndex,ArgsAndRefsSize);
        }

        internal override LeafElement Visit(ReplacePrimitiveRecursivity replacePrimitiveRecursivity)
        {
            return replacePrimitiveRecursivity.CallVisit(this);
        }

        public override string NodeDump { get { return base.NodeDump + " FunctionIndex="+FunctionIndex+" ArgsAndRefsSize="+ArgsAndRefsSize;  } }

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
    [Serializable]
    internal class RecursiveCallCandidate : LeafElement
    {
        [Node]
        private readonly Size RefsSize;

        protected override Size GetSize()
        {
            return Size.Zero;
        }

        protected override Size GetInputSize()
        {
            return RefsSize;
        }

        internal override LeafElement TryToCombineBack(TopFrame precedingElement)
        {
            if ((DeltaSize + precedingElement.DeltaSize).IsZero 
                && (precedingElement.Offset + RefsSize).IsZero)
                return new RecursiveCall();
            return base.TryToCombineBack(precedingElement);
        }

        public RecursiveCallCandidate(Size refsSize)
        {
            RefsSize = refsSize;
        }

    }

    [Serializable]
    internal class RecursiveCall : LeafElement
    {
        protected override Size GetSize()
        {
            return Size.Zero;
        }

        protected override Size GetInputSize()
        {
            return Size.Zero;
        }

        protected override string Format(StorageDescriptor start)
        {
            return StorageDescriptor.CreateRecursiveCall();
        }
    }
}
