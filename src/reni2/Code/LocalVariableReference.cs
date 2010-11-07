using System;
using HWClassLibrary.Debug;
using Reni.Context;

namespace Reni.Code
{
    internal sealed class LocalVariableReference : FiberHead
    {
        private readonly RefAlignParam _refAlignParam;
        private readonly string _holder;
        private readonly Size _offset;

        public LocalVariableReference(RefAlignParam refAlignParam, string holder, Size offset)
        {
            _refAlignParam = refAlignParam;
            _holder = holder;
            _offset = offset;
        }

        [IsDumpEnabled(false)]
        internal override RefAlignParam RefAlignParam { get { return _refAlignParam; } }
        protected override Size GetSize() { return _refAlignParam.RefSize; }
        [IsDumpEnabled(false)]
        public override string NodeDump { get { return base.NodeDump + " Holder=" + _holder + " Offset=" + _offset; } }
        [IsDumpEnabled(false)]
        internal string Holder { get { return _holder; } }
        [IsDumpEnabled(false)]
        internal Size Offset { get { return _offset; } }
        protected override CodeBase TryToCombine(FiberItem subsequentElement) { return subsequentElement.TryToCombineBack(this); }
    }
}