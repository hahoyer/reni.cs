namespace Reni.Code
{
    internal class BitArray : LeafElement
    {
        private readonly Size _size;
        private readonly BitsConst _data;

        public BitArray(Size size, BitsConst data)
        {
            _size = size;
            _data = data;
            StopByObjectId(-1095);
        }

        public BitsConst Data { get { return _data; } }
        public override Size Size { get { return _size; } }
        public override bool IsEmpty { get { return Data.IsEmpty; } }
        public override Size DeltaSize { get { return Size*(-1); } }

        protected override string Format(StorageDescriptor start)
        {
            if (Size.IsZero)
                return "";
            return start.BitsArray(Size, Data);
        }

        internal override LeafElement TryToCombine(LeafElement subsequentElement)
        {
            return subsequentElement.TryToCombineBack(this);
        }

        internal override BitsConst Evaluate()
        {
            return _data.Resize(_size);
        }

        public static BitArray CreateVoid()
        {
            return new BitArray(Size.Create(0),BitsConst.None());
        }

    }
}