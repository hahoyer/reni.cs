using System;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;

namespace Reni.Code
{
    /// <summary>
    /// Expression to change size of an expression
    /// </summary>
    [Serializable]
    internal sealed class BitCast : LeafElement
    {
        private static int _nextId;
        private readonly Size _size;
        [Node]
        private readonly Size SignificantSize;
        [Node]
        [DumpData(false)]
        internal readonly Size TargetSize;

        internal BitCast(Size size, Size targetSize, Size significantSize)
            : base(_nextId++)
        {
            _size = size;
            SignificantSize = significantSize.Min(size);
            TargetSize = targetSize;
            Tracer.Assert(TargetSize != Size || TargetSize != SignificantSize);
            StopByObjectId(-17);
        }

        protected override Size GetSize() { return _size; }

        protected override Size GetInputSize() { return TargetSize; }

        internal override LeafElement[] TryToCombineN(LeafElement subsequentElement) { return subsequentElement.TryToCombineBackN(this); }

        internal override LeafElement[] TryToCombineBackN(BitCast precedingElement)
        {
            if(precedingElement.Size != TargetSize)
                return null;
            var significantSize = SignificantSize.Min(precedingElement.SignificantSize);
            if (Size == TargetSize && Size == significantSize)
                return new LeafElement[0];
            if (Size == precedingElement.TargetSize && Size == significantSize)
                return new LeafElement[0];
            return new[] { new BitCast(Size, precedingElement.TargetSize, significantSize) };
        }

        internal override LeafElement TryToCombineBack(BitArray precedingElement)
        {
            var bitsConst = precedingElement.Data;
            if(bitsConst.Size > SignificantSize)
                bitsConst = bitsConst.Resize(SignificantSize);
            return new BitArray(Size, bitsConst);
        }

        public override string NodeDump { get { return base.NodeDump + " TargetSize=" + TargetSize + " SignificantSize=" + SignificantSize; } }

        internal override LeafElement[] TryToCombineBackN(TopData precedingElement)
        {
            if(precedingElement.Size == TargetSize && Size >= SignificantSize && Size > TargetSize)
                return new LeafElement[]
                {
                    new TopData(precedingElement.RefAlignParam, precedingElement.Offset, Size),
                    new BitCast(Size, Size, SignificantSize)
                };
            return null;
        }

        internal override LeafElement[] TryToCombineBackN(TopFrame precedingElement)
        {
            if(precedingElement.Size == TargetSize && Size >= SignificantSize && Size > TargetSize)
                return new LeafElement[]
                {
                    new TopFrame(precedingElement.RefAlignParam, precedingElement.Offset, Size),
                    new BitCast(Size, Size, SignificantSize)
                };
            return null;
        }

        internal override LeafElement[] TryToCombineBackN(BitArrayOp precedingElement)
        {
            if(TargetSize == Size)
                return null;

            var bitArrayOp = new BitArrayOp(precedingElement.OpToken, precedingElement.Size + Size - TargetSize,
                precedingElement.LeftSize, precedingElement.RightSize);

            if(SignificantSize == Size)
                return new LeafElement[] {bitArrayOp};

            return new LeafElement[] {bitArrayOp, new BitCast(Size, Size, SignificantSize)};
        }

        internal override LeafElement[] TryToCombineBackN(BitArrayPrefixOp precedingElement)
        {
            if (TargetSize == Size)
                return null;

            var bitArrayOp = new BitArrayPrefixOp(precedingElement.OpToken, precedingElement.Size + Size - TargetSize, precedingElement.ArgSize);

            if (SignificantSize == Size)
                return new LeafElement[] { bitArrayOp };

            return new LeafElement[] { bitArrayOp, new BitCast(Size, Size, SignificantSize) };
        }

        internal override LeafElement[] TryToCombineBackN(Dereference precedingElement)
        {
            if(precedingElement.Size == TargetSize && TargetSize != Size)
            {
                var dereference = new Dereference(precedingElement.RefAlignParam, Size);
                if (Size == SignificantSize)
                    return new LeafElement[] {dereference};
                return new LeafElement[] { dereference, new BitCast(Size, Size, SignificantSize) };
            }
            return null;
        }

        protected override string Format(StorageDescriptor start)
        {
            return start.CreateBitCast(TargetSize, Size, SignificantSize);
        }
    }
}