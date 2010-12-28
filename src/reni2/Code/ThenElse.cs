using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;

namespace Reni.Code
{
    /// <summary>
    /// Then-Else construct
    /// </summary>
    internal sealed class ThenElse : FiberItem
    {
        private static int _nextId;

        [Node]
        internal readonly CodeBase ThenCode;

        [Node]
        internal readonly CodeBase ElseCode;

        public ThenElse(CodeBase thenCode, CodeBase elseCode)
            : base(_nextId++)
        {
            ThenCode = thenCode;
            ElseCode = elseCode;
        }

        private Size MaxSizeImplementation
        {
            get
            {
                var tSize = ThenCode.MaxSize;
                var eSize = ElseCode.MaxSize;
                return tSize.Max(eSize);
            }
        }

        internal Refs RefsImplementation
        {
            get
            {
                return
                    ThenCode.RefsImplementation.Sequence(ElseCode.RefsImplementation);
            }
        }

        internal override Size InputSize { get { return Size.Create(1); } }
        internal override Size OutputSize { get { return ThenCode.Size; } }

        protected override void Execute(IFormalMaschine formalMaschine) { throw new NotImplementedException(); }
    }

    [Serializable]
    internal sealed class EndCondional : FiberItem
    {
        internal override Size InputSize { get { return Size.Zero; } }
        internal override Size OutputSize { get { return Size.Zero; } }
        protected override void Execute(IFormalMaschine formalMaschine) { throw new NotImplementedException(); }
    }

    internal sealed class Else : FiberItem
    {
        [Node, IsDumpEnabled(true)]
        private readonly Size _thenSize;

        public Else(Size thenSize) { _thenSize = thenSize; }

        internal override Size OutputSize { get { return Size.Zero; } }
        internal override Size InputSize { get { return _thenSize; } }
        protected override void Execute(IFormalMaschine formalMaschine) { throw new NotImplementedException(); }
    }

    [Serializable]
    internal sealed class Then : FiberItem
    {
        [Node]
        internal readonly Size CondSize;

        internal Then(Size condSize) { CondSize = condSize; }

        internal override Size OutputSize { get { return Size.Zero; } }
        internal override Size InputSize { get { return CondSize; } }

        internal override FiberItem[] TryToCombineBack(BitCast precedingElement)
        {
            if(precedingElement.OutputSize == CondSize)
                return new[] {new Then(precedingElement.TargetSize)};
            return null;
        }

        internal override FiberItem[] TryToCombineBack(BitArrayBinaryOp precedingElement)
        {
            if(precedingElement.OutputSize == CondSize)
                return new[] {new BitArrayOpThen(this, precedingElement)};
            return null;
        }

        protected override void Execute(IFormalMaschine formalMaschine) { throw new NotImplementedException(); }
    }

    [Serializable]
    internal sealed class BitArrayOpThen : FiberItem
    {
        [Node, IsDumpEnabled(true)]
        private readonly BitArrayBinaryOp _bitArrayBinaryOp;

        [Node, IsDumpEnabled(true)]
        private readonly Then _thenCode;

        public BitArrayOpThen(Then thenCode, BitArrayBinaryOp bitArrayBinaryOp)
        {
            _thenCode = thenCode;
            _bitArrayBinaryOp = bitArrayBinaryOp;
        }

        internal override Size InputSize { get { return _bitArrayBinaryOp.DeltaSize + _bitArrayBinaryOp.OutputSize; } }
        internal override Size OutputSize { get { return _thenCode.OutputSize; } }
        protected override void Execute(IFormalMaschine formalMaschine) { throw new NotImplementedException(); }
    }
}