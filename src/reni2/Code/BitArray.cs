using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using Reni.Basics;

namespace Reni.Code
{
    [Serializable]
    internal sealed class BitArray : FiberHead
    {
        private readonly Size _size;

        [Node, DisableDump]
        internal readonly BitsConst Data;

        public BitArray(Size size, BitsConst data)
        {
            //Tracer.Assert(size.IsPositive);
            _size = size;
            Data = data;
            StopByObjectId(-2);
        }

        protected override Size GetSize() { return _size; }

        protected override IEnumerable<CodeBase> AsList()
        {
            if(Size.IsZero)
                return new CodeBase[0];
            return new[] {this};
        }
        protected override TResult VisitImplementation<TResult>(Visitor<TResult> actual) { return actual.BitArray(this); }

        [DisableDump]
        internal override bool IsEmpty { get { return Size.IsZero; } }

        protected override CodeBase TryToCombine(FiberItem subsequentElement) { return subsequentElement.TryToCombineBack(this); }

        internal override void Visit(IVisitor visitor) { visitor.BitsArray(Size, Data); }

        [DisableDump]
        public override string NodeDump { get { return base.NodeDump + " Data=" + Data; } }

        protected override string CSharpString(Size top) { return CSharpGenerator.Push(top, Size, Data); }

        internal new static BitArray Void() { return new BitArray(Size.Create(0), Basics.BitsConst.None()); }
    }
}