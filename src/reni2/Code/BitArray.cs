using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;

namespace Reni.Code
{
    [Serializable]
    internal class BitArray : FiberHead
    {
        private readonly Size _size;

        [Node, IsDumpEnabled(false)]
        internal readonly BitsConst Data;

        public BitArray(Size size, BitsConst data)
        {
            _size = size;
            Data = data;
            StopByObjectId(-527);
        }

        protected override Size GetSize() { return _size; }

        protected override IEnumerable<CodeBase> AsList()
        {
            if(Size.IsZero)
                return new CodeBase[0];
            return new[]{this};
        }

        [IsDumpEnabled(false)]
        internal override bool IsEmpty { get { return Data.IsEmpty; } }

        protected override CodeBase TryToCombine(FiberItem subsequentElement) { return subsequentElement.TryToCombineBack(this); }
        internal override BitsConst Evaluate() { return Data.Resize(_size); }
        internal override void Execute(IFormalMaschine formalMaschine) { formalMaschine.BitsArray(Size, Data); }

        [IsDumpEnabled(false)]
        public override string NodeDump { get { return base.NodeDump + " Data=" + Data; } }

        internal override string Format(StorageDescriptor start)
        {
            return StorageDescriptor.CreateBitsArray(GetSize(), Data);
        }

        internal new static BitArray Void() { return new BitArray(Size.Create(0), Reni.BitsConst.None()); }
    }
}