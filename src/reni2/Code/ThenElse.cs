using HWClassLibrary.TreeStructure;
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

        protected override Size SizeImplementation { get { return ThenCode.Size; } }

        internal int ThenElseObjectId { get { return _thenElseObjectId; } }

        protected override Size MaxSizeImplementation
        {
            get
            {
                var cSize = CondCode.MaxSize;
                var tSize = ThenCode.MaxSize;
                var eSize = ElseCode.MaxSize;
                return cSize.Max(tSize).Max(eSize);
            }
        }

        public override Result VisitImplementation<Result>(Visitor<Result> actual)
        {
            return actual.ThenElseVisit(this);
        }

        internal override Refs RefsImplementation
        {
            get
            {
                return
                    CondCode.RefsImplementation.CreateSequence(ThenCode.RefsImplementation).CreateSequence(
                        ElseCode.RefsImplementation);
            }
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

        protected override Size GetInputSize()
        {
            return Size.Zero;
        }

        protected override Size GetSize()
        {
            return Size.Zero;
        }

        protected override string Format(StorageDescriptor start)
        {
            return StorageDescriptor.CreateEndCondional(_thenElseObjectId);
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

        protected override Size GetInputSize()
        {
            return _thenSize;
        }

        protected override string Format(StorageDescriptor start)
        {
            return StorageDescriptor.CreateElse(_thenElseObjectId);
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

        protected override Size GetInputSize()
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

        internal override LeafElement TryToCombineBack(BitArrayBinaryOp precedingElement)
        {
            if(precedingElement.Size == CondSize)
                return new BitArrayOpThen(this, precedingElement);
            return null;
        }

        protected override string Format(StorageDescriptor start)
        {
            return start.CreateThen(ThenElseObjectId, CondSize);
        }
    }

    [Serializable]
    internal class BitArrayOpThen : LeafElement
    {
        [Node, DumpData(true)]
        private readonly BitArrayBinaryOp _bitArrayBinaryOp;
        [Node, DumpData(true)]
        private readonly Then _thenCode;

        public BitArrayOpThen(Then thenCode, BitArrayBinaryOp bitArrayBinaryOp)
        {
            _thenCode = thenCode;
            _bitArrayBinaryOp = bitArrayBinaryOp;
        }

        protected override Size GetSize()
        {
            return _thenCode.Size;
        }

        protected override Size GetInputSize()
        {
            return _bitArrayBinaryOp.DeltaSize + _bitArrayBinaryOp.Size;
        }

        protected override string Format(StorageDescriptor start)
        {
            return start.CreateBitArrayOpThen(_bitArrayBinaryOp.OpToken, _bitArrayBinaryOp.LeftSize, _bitArrayBinaryOp.RightSize, _thenCode.ThenElseObjectId, _thenCode.CondSize);
        }
    }
}