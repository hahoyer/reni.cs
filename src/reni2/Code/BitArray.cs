using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using Reni.Context;

namespace Reni.Code
{
    [Serializable]
    internal class BitArray : StartingLeafElement
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

        [IsDumpEnabled(false)]
        internal override bool IsEmpty { get { return Data.IsEmpty; } }
        internal override LeafElement TryToCombine(LeafElement subsequentElement) { return subsequentElement.TryToCombineBack(this); }
        internal override BitsConst Evaluate() { return Data.Resize(_size); }
        internal override void Execute(IFormalMaschine formalMaschine) { formalMaschine.BitsArray(Size, Data); }
        [IsDumpEnabled(false)]
        public override string NodeDump { get { return base.NodeDump + " Data=" + Data; } }

        protected override string Format(StorageDescriptor start)
        {
            if(GetSize().IsZero)
                return "";
            return start.CreateBitsArray(GetSize(), Data);
        }

        internal static BitArray Void() { return new BitArray(Size.Create(0), BitsConst.None()); }
    }
}