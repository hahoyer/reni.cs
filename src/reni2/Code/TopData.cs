using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Context;

namespace Reni.Code
{
    /// <summary>
    ///     Combination of TopRef and Unref
    /// </summary>
    [Serializable]
    internal sealed class TopData : Top
    {
        public TopData(RefAlignParam refAlignParam, Size offset, Size size, Size dataSize)
            : base(refAlignParam, offset, size, dataSize) { StopByObjectId(-110); }

        protected override CodeBase TryToCombine(FiberItem subsequentElement)
        {
            var fiber = subsequentElement.TryToCombineBack(this);
            if(fiber == null)
                return null;
            return fiber;
        }

        protected override void Execute(IFormalMaschine formalMaschine) { formalMaschine.TopData(Offset, Size, DataSize); }

        protected override string CSharpString() { return CSharpGenerator.TopData(Offset, GetSize()); }
    }

    /// <summary>
    ///     Combination of TopFrameRef and Unref
    /// </summary>
    [Serializable]
    internal sealed class TopFrameData : Top
    {
        public TopFrameData(RefAlignParam refAlignParam, Size offset, Size size, Size dataSize)
            : base(refAlignParam, offset, size, dataSize) { StopByObjectId(544); }

        protected override CodeBase TryToCombine(FiberItem subsequentElement)
        {
            var fiber = subsequentElement.TryToCombineBack(this);
            if(fiber == null)
                return null;
            return fiber;
        }

        protected override void Execute(IFormalMaschine formalMaschine) { formalMaschine.TopFrameData(Offset, Size, DataSize); }

        protected override string CSharpString() { return CSharpGenerator.TopFrame(Offset, GetSize()); }
    }
}