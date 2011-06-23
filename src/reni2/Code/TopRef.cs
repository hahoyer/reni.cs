using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Context;

namespace Reni.Code
{
    [Serializable]
    internal sealed class TopRef : Ref
    {
        public TopRef(RefAlignParam refAlignParam, Size size, string reason)
            : base(refAlignParam, size, reason) { StopByObjectId(21); }

        public TopRef(RefAlignParam refAlignParam, string reason)
            : this(refAlignParam, Size.Zero, reason) { }

        protected override string CSharpString(Size top) { return CSharpGenerator.TopRef(top, Size); }

        protected override CodeBase TryToCombine(FiberItem subsequentElement) { return subsequentElement.TryToCombineBack(this); }

        protected override void Execute(IFormalMaschine formalMaschine) { formalMaschine.TopRef(Offset, Size); }
    }

    [Serializable]
    internal sealed class TopFrameRef : Ref
    {
        public TopFrameRef(RefAlignParam refAlignParam, string reason)
            : this(refAlignParam, Size.Zero, reason) { StopByObjectId(547); }

        public TopFrameRef(RefAlignParam refAlignParam, Size offset, string reason)
            : base(refAlignParam, offset, reason) { StopByObjectId(547); }

        protected override string CSharpString() { return CSharpGenerator.CreateFrameRef(RefAlignParam, Offset); }

        protected override CodeBase TryToCombine(FiberItem subsequentElement) { return subsequentElement.TryToCombineBack(this); }
    }
}