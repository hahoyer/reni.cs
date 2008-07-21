using HWClassLibrary.Debug;
using HWClassLibrary.Helper;

namespace Reni.Code
{
    /// <summary>
    /// Expression to change size of an expression
    /// </summary>
    internal sealed class BitCast : LeafElement
    {
        private readonly Size _size;
        [Node]
        private readonly Size SignificantSize;
        [Node]
        internal readonly Size TargetSize;

        internal BitCast(Size targetSize, Size size, Size significantSize)
        {
            _size = size;
            SignificantSize = significantSize;
            TargetSize = targetSize;
        }

        protected override Size GetSize()
        {
            return _size;
        }

        protected override Size GetDeltaSize()
        {
            return TargetSize - GetSize();
        }

        internal override LeafElement TryToCombine(LeafElement subsequentElement)
        {
            if(GetSize() == TargetSize && GetSize() == SignificantSize)
                return subsequentElement;
            return subsequentElement.TryToCombineBack(this);
        }

        internal override LeafElement TryToCombineBack(BitCast precedingElement)
        {
            if(precedingElement.GetSize() != TargetSize)
                return null;

            return new BitCast
                (
                precedingElement.TargetSize,
                GetSize(),
                SignificantSize.Min(precedingElement.SignificantSize)
                );
        }

        internal override LeafElement TryToCombineBack(BitArray precedingElement)
        {
            var bitsConst = precedingElement.Data;
            if(bitsConst.Size > SignificantSize)
                bitsConst = bitsConst.Resize(SignificantSize);
            return new BitArray(GetSize(), bitsConst);
        }

        public override string NodeDump { get { return base.NodeDump + "TargetSize="+TargetSize+" SignificantSize="+SignificantSize; } }

        internal override LeafElement[] TryToCombineBackN(TopData precedingElement)
        {
            if (precedingElement.Size == TargetSize && GetSize() >= SignificantSize && GetSize() > TargetSize)
                return new LeafElement[]
                           {
                               new TopData(precedingElement.RefAlignParam, precedingElement.Offset, GetSize()),
                               new BitCast(GetSize(), GetSize(), SignificantSize)
                           };
            return null;
        }

        internal override LeafElement[] TryToCombineBackN(TopFrame precedingElement)
        {
            if (precedingElement.Size == TargetSize && GetSize() >= SignificantSize && GetSize() > TargetSize)
                return new LeafElement[]
                           {
                               new TopFrame(precedingElement.RefAlignParam, precedingElement.Offset, GetSize()),
                               new BitCast(GetSize(), GetSize(), SignificantSize)
                           };
            return null;
        }

        internal override LeafElement[] TryToCombineBackN(BitArrayOp precedingElement)
        {
            if (TargetSize == GetSize() && TargetSize == SignificantSize)
                return new LeafElement[]{precedingElement};
            if (TargetSize != GetSize())
                return new LeafElement[]
                           {
                               new BitArrayOp(precedingElement.OpToken, precedingElement.Size + GetSize() - TargetSize,
                                              precedingElement.LeftSize, precedingElement.RightSize),
                               new BitCast(GetSize(), GetSize(), SignificantSize)
                           };
            return null;
        }

        internal override LeafElement TryToCombineBack(Dereference precedingElement)
        {
            if (GetSize() == TargetSize)
            {
                Tracer.Assert(TargetSize == precedingElement.Size);
                return precedingElement;
            }
            return null;
        }

        protected override string Format(StorageDescriptor start)
        {
            return start.BitCast(TargetSize, GetSize(), SignificantSize);
        }
    }
}