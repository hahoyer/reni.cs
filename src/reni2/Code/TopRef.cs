using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Context;

namespace Reni.Code
{
    [Serializable]
    internal sealed class TopRef : Ref
    {
        public TopRef(RefAlignParam refAlignParam, Size offset, string reason)
            : base(refAlignParam, offset,reason)
        {
            StopByObjectId(530);
        }

        protected override string CSharpString()
        {
            return CSharpGenerator.CreateTopRef(RefAlignParam, Offset);
        }

        protected override CodeBase TryToCombine(FiberItem subsequentElement)
        {
            return subsequentElement.TryToCombineBack(this);
        }

        internal override void Execute(IFormalMaschine formalMaschine) { formalMaschine.TopRef(RefAlignParam, Offset); }
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