using System.Collections.Generic;
using hw.DebugFormatter;

using Reni.Basics;

namespace Reni.Code
{
    sealed class BitArray : FiberHead
    {
        readonly Size _size;

        [Node]
        [DisableDump]
        internal readonly BitsConst Data;

        public BitArray(Size size, BitsConst data)
        {
            //Tracer.Assert(size.IsPositive);
            _size = size;
            Data = data;
            StopByObjectIds();
        }

        public BitArray()
            : this(Size.Zero, Basics.BitsConst.None()) {}

        protected override Size GetSize() => _size;

        protected override IEnumerable<CodeBase> AsList()
        {
            if(Hllw)
                return new CodeBase[0];
            return new[] {this};
        }

        protected override TCode VisitImplementation<TCode, TFiber>(Visitor<TCode, TFiber> actual)
            => actual.BitArray(this);

        [DisableDump]
        internal override bool IsEmpty => Hllw;

        protected override CodeBase TryToCombine(FiberItem subsequentElement)
            => subsequentElement.TryToCombineBack(this);

        internal override void Visit(IVisitor visitor) => visitor.BitsArray(Size, Data);

        protected override string GetNodeDump() => base.GetNodeDump() + " Data=" + Data;

        internal new static BitArray Void => new BitArray(Size.Create(0), Basics.BitsConst.None());
    }
}