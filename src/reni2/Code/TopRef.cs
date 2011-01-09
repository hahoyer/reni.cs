using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Context;

namespace Reni.Code
{
    [Serializable]
    internal sealed class TopRef : Ref
    {
        public TopRef(RefAlignParam refAlignParam, string reason)
            : base(refAlignParam, Reni.Size.Zero,reason)
        {
            StopByObjectId(1160);
        }

        protected override string CSharpString(Size top) { return CSharpGenerator.TopRef(top, Size); }

        protected override CodeBase TryToCombine(FiberItem subsequentElement)
        {
            return subsequentElement.TryToCombineBack(this);
        }

        protected override void Execute(IFormalMaschine formalMaschine) { formalMaschine.TopRef(RefAlignParam, Offset); }
    }

    [Serializable]
    internal sealed class FrameRef : Ref
    {
        public FrameRef(RefAlignParam refAlignParam, Size offset, string reason)
            : base(refAlignParam, offset, reason)
        {
            StopByObjectId(547);
        }

        protected override string CSharpString()
        {
            return CSharpGenerator.CreateFrameRef(RefAlignParam, Offset);
        }

        protected override CodeBase TryToCombine(FiberItem subsequentElement)
        {
            return subsequentElement.TryToCombineBack(this);
        }
    }
}