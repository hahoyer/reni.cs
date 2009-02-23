using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;

namespace Reni.Code
{
    [Serializable]
    internal class BitArray : LeafElement
    {
        private readonly Size _size;
        [Node]
        internal readonly BitsConst Data;

        public BitArray(Size size, BitsConst data)
        {
            _size = size;
            Data = data;
            StopByObjectId(-527);
        }

        protected override Size GetSize()
        {
            return _size;
        }

        [DumpData(false)]
        internal override bool IsEmpty { get { return Data.IsEmpty; } }

        protected override Size GetInputSize()
        {
            return Size.Zero;
        }

        protected override string Format(StorageDescriptor start)
        {
            if (GetSize().IsZero)
                return "";
            return start.CreateBitsArray(GetSize(), Data);
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