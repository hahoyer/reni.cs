using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Basics;

namespace Reni.Code
{
    sealed class UnknownStackData : NonListStackData
    {
        public UnknownStackData(Size size, IOutStream outStream)
            : base(outStream)
        {
            Size = size;
        }

        internal override Size Size { get; }

        internal override StackData PushOnto(ListStack formerStack)
            => new ListStack(this, formerStack);

        sealed class GetBitsConstException : Exception
        {
            public GetBitsConstException()
                : base("GetBitsConst failed")
            { }
        }


        internal sealed class GetTopException : Exception
        {
            public GetTopException(Size size)
                : base("GetTop failed for size = " + size.Dump())
            { }
        }

        internal sealed class PullException : Exception
        {
            public PullException(Size size)
                : base("Pull failed for size = " + size.Dump())
            { }
        }


        internal sealed class StackDataAddressException : Exception
        {
            public StackDataAddressException()
                : base("StackDataAddress failed")
            { }
        }

        protected override StackDataAddress GetAddress() { throw new StackDataAddressException(); }
        internal override BitsConst GetBitsConst() { throw new GetBitsConstException(); }
        protected override StackData GetTop(Size size) { throw new GetTopException(size); }
        protected override StackData Pull(Size size) { throw new PullException(size); }
    }
}