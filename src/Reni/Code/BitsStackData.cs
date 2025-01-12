using Reni.Basics;

namespace Reni.Code
{
    internal sealed class BitsStackData : NonListStackData
    {
        [EnableDump]
        private readonly BitsConst Data;

        public BitsStackData(BitsConst data, IOutStream outStream)
            : base(outStream)
        {
            Data = data;
            (!Data.Size.IsZero).Assert();
        }

        protected override string Dump(bool isRecursion) => Data.DumpValue();

        internal override BitsConst GetBitsConst() => Data;
        protected override StackData GetTop(Size size)
        {
            if (size < Size)
                return BitCast(size);
            return base.GetTop(size);
        }

        protected override StackData Pull(Size size)
        {
            if(size <= Size)
                return new BitsStackData(Data.ShiftDown(size),OutStream);
            return base.Pull(size);
        }
        internal override Size Size => GetBitsConst().Size;
    }
}