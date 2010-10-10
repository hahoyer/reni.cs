using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.TreeStructure;
using Reni.Context;

namespace Reni.Code
{
    [Serializable]

    sealed internal class Call : FiberItem
    {
        [Node,IsDumpEnabled(false)]
        internal readonly int FunctionIndex;
        [Node, IsDumpEnabled(false)]
        private readonly Size ResultSize;
        [Node, IsDumpEnabled(false)]
        internal readonly Size ArgsAndRefsSize;

        internal Call(int functionIndex, Size resultSize, Size argsAndRefsSize)
        {
            FunctionIndex = functionIndex;
            ResultSize = resultSize;
            ArgsAndRefsSize = argsAndRefsSize;
        }

        internal override Size InputSize { get { return ArgsAndRefsSize; } }
        internal override Size OutputSize { get { return ResultSize; } }

        protected override string Format(StorageDescriptor start)
        {
            return start.CreateCall(FunctionIndex,ArgsAndRefsSize);
        }

        internal FiberItem Visit(ReplacePrimitiveRecursivity replacePrimitiveRecursivity)
        {
            return replacePrimitiveRecursivity.CallVisit(this);
        }

        [IsDumpEnabled(false)]
        public override string NodeDump { get { return base.NodeDump + " FunctionIndex=" + FunctionIndex + " ArgsAndRefsSize=" + ArgsAndRefsSize; } }

        internal override void Execute(IFormalMaschine formalMaschine) { formalMaschine.Call(OutputSize, FunctionIndex, ArgsAndRefsSize); }

        public FiberItem TryConvertToRecursiveCall(int functionIndex)
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
    internal class RecursiveCallCandidate : FiberItem
    {
        [Node]
        private readonly Size _refsSize;

        internal override Size InputSize { get { return _refsSize; } }

        internal override Size OutputSize { get { return Size.Zero; } }

        protected override string Format(StorageDescriptor start) { throw new NotImplementedException(); }

        internal override void Execute(IFormalMaschine formalMaschine) { throw new NotImplementedException(); }

        internal override CodeBase TryToCombineBack(TopFrame precedingElement)
        {
            if ((DeltaSize + precedingElement.Size).IsZero 
                && (precedingElement.Offset + _refsSize).IsZero)
                return new RecursiveCall();
            return base.TryToCombineBack(precedingElement);
        }

        internal RecursiveCallCandidate(Size refsSize)
        {
            _refsSize = refsSize;
        }

    }

    [Serializable]
    internal class RecursiveCall : FiberHead
    {
        protected override Size GetSize() { return Size.Zero; }
        protected override string Format(StorageDescriptor start) { return StorageDescriptor.CreateRecursiveCall(); }
    }
}
