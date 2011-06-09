using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Context;

namespace Reni.Code
{
    internal sealed class LocalVariableReference : FiberHead
    {
        private readonly RefAlignParam _refAlignParam;
        private readonly string _holder;
        private readonly Size _offset;
        private static int _nextObjectId;

        public LocalVariableReference(RefAlignParam refAlignParam, string holder, Size offset)
            : base(_nextObjectId++)
        {
            _refAlignParam = refAlignParam;
            _holder = holder;
            _offset = offset ?? Size.Zero;
            //StopByObjectId(0);
        }

        [DisableDump]
        internal override RefAlignParam RefAlignParam { get { return _refAlignParam; } }

        protected override Size GetSize() { return _refAlignParam.RefSize; }

        [DisableDump]
        public override string NodeDump { get { return base.NodeDump + " Holder=" + _holder + " Offset=" + _offset; } }

        protected override string CSharpString(Size top) { return CSharpGenerator.LocalVariableReference(top, Size, _holder, _offset); }
        protected override void Execute(IFormalMaschine formalMaschine) { formalMaschine.LocalVariableReference(Size, _holder, _offset); }

        [DisableDump]
        internal string Holder { get { return _holder; } }

        [DisableDump]
        internal Size Offset { get { return _offset; } }

        protected override CodeBase TryToCombine(FiberItem subsequentElement) { return subsequentElement.TryToCombineBack(this); }
    }
}