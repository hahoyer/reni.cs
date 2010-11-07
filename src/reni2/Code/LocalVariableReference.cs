using System;
using HWClassLibrary.Debug;
using Reni.Context;

namespace Reni.Code
{
    internal sealed class LocalVariableReference : FiberHead
    {
        private readonly RefAlignParam _refAlignParam;
        private readonly string _holder;

        public LocalVariableReference(RefAlignParam refAlignParam, string holder)
        {
            _refAlignParam = refAlignParam;
            _holder = holder;
        }

        [IsDumpEnabled(false)]
        internal override RefAlignParam RefAlignParam { get { return _refAlignParam; } }
        protected override Size GetSize() { return _refAlignParam.RefSize; }
        [IsDumpEnabled(false)]
        public override string NodeDump { get { return base.NodeDump + " Holder=" + _holder; } }
        [IsDumpEnabled(false)]
        internal string Holder { get { return _holder; } }

        protected override CodeBase TryToCombine(FiberItem subsequentElement) { return subsequentElement.TryToCombineBack(this); }
    }
}