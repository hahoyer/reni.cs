using HWClassLibrary.Helper;

namespace Reni.Code
{
    internal class BitArray : LeafElement
    {
        private readonly Size _size;
        [Node]
        internal readonly BitsConst Data;

        public BitArray(Size size, BitsConst data)
        {
            _size = size;
            Data = data;
            StopByObjectId(-9814);
        }

        protected override Size GetSize()
        {
            return _size;
        }

        internal override bool IsEmpty { get { return Data.IsEmpty; } }

        protected override Size GetDeltaSize()
        {
            return GetSize()*(-1);
        }

        protected override string Format(StorageDescriptor start)
        {
            if (GetSize().IsZero)
                return "";
            return start.BitsArray(GetSize(), Data);
        }

        internal override LeafElement TryToCombine(LeafElement subsequentElement)
        {
            return subsequentElement.TryToCombineBack(this);
        }

        internal override BitsConst Evaluate()
        {
            return Data.Resize(_size);
        }

        public override string NodeDump { get { return base.NodeDump + " Data="+Data; } }

        public static BitArray CreateVoid()
        {
            return new BitArray(Size.Create(0),BitsConst.None());
        }

    }
}