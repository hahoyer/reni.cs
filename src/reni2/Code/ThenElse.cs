using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;

namespace Reni.Code
{
    /// <summary>
    /// Then-Else construct
    /// </summary>
    internal sealed class ThenElse : CodeBase
    {
        private static int _nextId;
        private readonly int _thenElseObjectId = _nextId++;

        [Node]
        internal readonly CodeBase CondCode;
        [Node]
        internal readonly CodeBase ThenCode;
        [Node]
        internal readonly CodeBase ElseCode;

        public ThenElse(CodeBase condCode, CodeBase thenCode, CodeBase elseCode)
        {
            CondCode = condCode;
            ThenCode = thenCode;
            ElseCode = elseCode;
        }

        internal protected override Size GetSize()
        {
            return ThenCode.Size;
        }

        internal int ThenElseObjectId { get { return _thenElseObjectId; } }

        internal protected override Size GetMaxSize()
        {
            var cSize = CondCode.MaxSize;
            var tSize = ThenCode.MaxSize;
            var eSize = ElseCode.MaxSize;
            return cSize.Max(tSize).Max(eSize);
        }

        public override Result VirtVisit<Result>(Visitor<Result> actual)
        {
            return actual.ThenElseVisit(this);
        }
    }

    [Serializable]
    internal sealed class EndCondional : LeafElement
    {
        [Node, DumpData(true)]
        private readonly int _thenElseObjectId;

        internal EndCondional(int thenElseObjectId)
        {
            _thenElseObjectId = thenElseObjectId;
        }

        protected override Size GetDeltaSize()
        {
            return Size.Zero;
        }

        protected override Size GetSize()
        {
            return Size.Zero;
        }

        protected override string Format(StorageDescriptor start)
        {
            return start.EndCondional(_thenElseObjectId);
        }
    }
    [Serializable]

    internal sealed class Else : LeafElement
    {
        [Node, DumpData(true)]
        private readonly int _thenElseObjectId;
        [Node, DumpData(true)]
        private readonly Size _thenSize;

        public Else(int thenElseObjectId, Size thenSize)
        {
            _thenElseObjectId = thenElseObjectId;
            _thenSize = thenSize;
        }

        protected override Size GetSize()
        {
            return Size.Zero;
        }

        protected override Size GetDeltaSize()
        {
            return _thenSize;
        }

        protected override string Format(StorageDescriptor start)
        {
            return start.Else(_thenElseObjectId);
        }
    }

    [Serializable]
    internal sealed class Then : LeafElement
    {
        [Node]
        internal readonly int ThenElseObjectId;
        [Node]
        internal readonly Size CondSize;

        public Then(int thenElseObjectId, Size condSize)
        {
            ThenElseObjectId = thenElseObjectId;
            CondSize = condSize;
        }

        protected override Size GetDeltaSize()
        {
            return CondSize;
        }

        protected override Size GetSize()
        {
            return Size.Zero;
        }

        internal override LeafElement TryToCombineBack(BitCast precedingElement)
        {
            if(precedingElement.Size == CondSize)
                return new Then(ThenElseObjectId, precedingElement.TargetSize);
            return null;
        }

        internal override LeafElement TryToCombineBack(BitArrayOp precedingElement)
        {
            if(precedingElement.Size == CondSize)
                return new BitArrayOpThen(this, precedingElement);
            return null;
        }

        protected override string Format(StorageDescriptor start)
        {
            return start.Then(ThenElseObjectId, CondSize);
        }
    }

    [Serializable]
    internal class BitArrayOpThen : LeafElement
    {
        [Node, DumpData(true)]
        private readonly BitArrayOp _bitArrayOp;
        [Node, DumpData(true)]
        private readonly Then _thenCode;

        public BitArrayOpThen(Then thenCode, BitArrayOp bitArrayOp)
        {
            _thenCode = thenCode;
            _bitArrayOp = bitArrayOp;
        }

        protected override Size GetSize()
        {
            return _bitArrayOp.DeltaSize;
        }

        protected override Size GetDeltaSize()
        {
            return _thenCode.DeltaSize + _bitArrayOp.DeltaSize;
        }

        protected override string Format(StorageDescriptor start)
        {
            return start.BitArrayOpThen(_bitArrayOp.OpToken, _bitArrayOp.LeftSize, _bitArrayOp.RightSize, _thenCode.ThenElseObjectId, _thenCode.CondSize);
        }
    }
}