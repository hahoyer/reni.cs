using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Context;

namespace Reni.Code
{
    internal sealed class LocalVariableAccess : FiberHead
    {
        private static int _nextObjectId;
        private readonly RefAlignParam _refAlignParam;
        [DisableDump]
        internal readonly string Holder;
        [DisableDump]
        internal readonly Size Offset;
        private readonly Size _size;
        [DisableDump]
        internal readonly Size DataSize;

        public LocalVariableAccess(RefAlignParam refAlignParam, string holder, Size offset, Size size, Size dataSize)
            : base(_nextObjectId++)
        {
            _refAlignParam = refAlignParam;
            Holder = holder;
            Offset = offset;
            _size = size;
            DataSize = dataSize;
            StopByObjectId(-10);
        }

        [DisableDump]
        internal override RefAlignParam RefAlignParam { get { return _refAlignParam; } }

        [DisableDump]
        public override string NodeDump
        {
            get
            {
                return base.NodeDump
                       + " Holder=" + Holder
                       + " Offset=" + Offset
                       + " Size=" + _size
                       + " DataSize=" + DataSize
                    ;
            }
        }

        protected override Size GetSize() { return _size; }
        protected override string CSharpString() { return CSharpGenerator.LocalVariableAccess(Holder, Offset, _size); }
        protected override void Execute(IFormalMaschine formalMaschine) { formalMaschine.LocalVariableData(Size, Holder, Offset, DataSize); }
        protected override CodeBase TryToCombine(FiberItem subsequentElement) { return subsequentElement.TryToCombineBack(this); }
    }
}