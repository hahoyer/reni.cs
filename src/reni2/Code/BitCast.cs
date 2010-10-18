using System;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;

namespace Reni.Code
{
    /// <summary>
    /// Expression to change size of an expression
    /// </summary>
    [Serializable]
    internal sealed class BitCast : FiberItem
    {
        private static int _nextId;
        private readonly Size _size;
        [Node]
        private readonly Size _significantSize;
        [Node]
        [IsDumpEnabled(false)]
        internal readonly Size TargetSize;

        internal BitCast(Size size, Size targetSize, Size significantSize)
            : base(_nextId++)
        {
            Tracer.Assert(size != targetSize || targetSize != significantSize);
            _size = size;
            _significantSize = significantSize.Min(size);
            TargetSize = targetSize;
            StopByObjectId(-17);
        }

        [IsDumpEnabled(false)]
        internal override Size OutputSize { get { return _size; } }
        [IsDumpEnabled(false)]
        internal override Size InputSize { get { return TargetSize; } }

        internal override FiberItem[] TryToCombine(FiberItem subsequentElement) { return subsequentElement.TryToCombineBack(this); }

        internal override FiberItem[] TryToCombineBack(BitCast precedingElement)
        {
            if(precedingElement._size != TargetSize)
                return null;
            var significantSize = _significantSize.Min(precedingElement._significantSize);
            if (_size == TargetSize && _size == significantSize)
                return new FiberItem[0];
            if (_size == precedingElement.TargetSize && _size == significantSize)
                return new FiberItem[0];
            return new[] { new BitCast(_size, precedingElement.TargetSize, significantSize) };
        }

        internal override CodeBase TryToCombineBack(BitArray precedingElement)
        {
            var bitsConst = precedingElement.Data;
            if(bitsConst.Size > _significantSize)
                bitsConst = bitsConst.Resize(_significantSize);
            return new BitArray(_size, bitsConst);
        }

        [IsDumpEnabled(false)]
        public override string NodeDump { get { return base.NodeDump + " TargetSize=" + TargetSize + " SignificantSize=" + _significantSize; } }
        internal override void Execute(IFormalMaschine formalMaschine) { formalMaschine.BitCast(_size, TargetSize, _significantSize); }

        internal override CodeBase TryToCombineBack(TopData precedingElement)
        {
            if(precedingElement.Size == TargetSize && _size >= _significantSize && _size > TargetSize)
                return (new TopData(precedingElement.RefAlignParam, precedingElement.Offset, _size, _significantSize))
                    .CreateFiber(new BitCast(_size, _size, _significantSize));
            return null;
        }

        internal override CodeBase TryToCombineBack(TopFrame precedingElement)
        {
            if(precedingElement.Size == TargetSize && _size >= _significantSize && _size > TargetSize)
                return new TopFrame(precedingElement.RefAlignParam, precedingElement.Offset, _size, _significantSize)
                    .CreateFiber(new BitCast(_size, _size, _significantSize));
            return null;
        }

        internal override FiberItem[] TryToCombineBack(BitArrayBinaryOp precedingElement)
        {
            if(TargetSize == _size)
                return null;

            FiberItem bitArrayOp = new BitArrayBinaryOp(
                precedingElement.OpToken, 
                precedingElement.OutputSize + _size - TargetSize,
                precedingElement.LeftSize, 
                precedingElement.RightSize);

            if(_significantSize == _size)
                return new[] {bitArrayOp};

            return new[] {bitArrayOp, new BitCast(_size, _size, _significantSize)};
        }

        internal override FiberItem[] TryToCombineBack(BitArrayPrefixOp precedingElement)
        {
            if (TargetSize == _size)
                return null;

            var bitArrayOp = new BitArrayPrefixOp(
                precedingElement.OpToken, 
                precedingElement.OutputSize + _size - TargetSize, 
                precedingElement.ArgSize);

            if (_significantSize == _size)
                return new FiberItem[] { bitArrayOp };

            return new FiberItem[] { bitArrayOp, new BitCast(_size, _size, _significantSize) };
        }

        internal override FiberItem[] TryToCombineBack(Dereference precedingElement)
        {
            if(precedingElement.OutputSize == TargetSize && TargetSize != _size)
            {
                var dereference = new Dereference(precedingElement.RefAlignParam, _size,precedingElement.DataSize);
                if (_size == _significantSize)
                    return new FiberItem[] { dereference };
                return new FiberItem[] { dereference, new BitCast(_size, _size, _significantSize) };
            }
            return null;
        }

    }
}