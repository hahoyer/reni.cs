using System;
using hw.DebugFormatter;
using Reni.Basics;

namespace Reni.Code
{
    sealed class StackDataAddress : NonListStackData
    {
        [EnableDump]
        readonly IStackDataAddressBase _data;
        [EnableDump]
        readonly Size _offset;

        public StackDataAddress(IStackDataAddressBase data, Size offset, IOutStream outStream)
            : base(outStream)
        {
            _data = data;
            _offset = offset;
        }

        protected override StackData GetTop(Size size) { throw new GetTopException(size); }
        protected override StackData Pull(Size size) { throw new PullException(size); }

        internal sealed class GetTopException : Exception
        {
            public GetTopException(Size size)
                : base("GetTop failed for size = " + size.Dump())
            { }
        }

        internal sealed class PullException : Exception
        {
            public PullException(Size size)
                : base("Pull failed for size = " + size.Dump()) { }
        }

        internal override Size Size => DataStack.RefSize;

        internal new StackData Dereference(Size size, Size dataSize)
            => _data.GetTop(_offset, size).BitCast(dataSize).BitCast(size);

        internal new void Assign(Size size, StackData right)
            => _data.SetTop(_offset, right.Dereference(size, size));

        internal override StackData BitArrayBinaryOp(string opToken, Size size, StackData right)
        {
            if(size == DataStack.RefSize && opToken == "Plus")
                return RefPlus(Size.Create(right.GetBitsConst().ToInt32()));


            NotImplementedMethod(opToken, size, right);
            return null;
        }

        internal new StackData RefPlus(Size offset)
        {
            if(offset.IsZero)
                return this;
            return new StackDataAddress(_data, offset + _offset, OutStream);
        }

        protected override StackDataAddress GetAddress() => this;

        protected override string Dump(bool isRecursion)
        {
            if(isRecursion)
                throw new NotImplementedException();
            return _data.Dump() + "[" + _offset.ToInt() + "]";
        }
    }

    interface IStackDataAddressBase
    {
        StackData GetTop(Size offset, Size size);
        string Dump();
        void SetTop(Size offset, StackData right);
    }
}