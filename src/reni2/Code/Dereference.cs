using System;
using HWClassLibrary.Debug;
using Reni.Context;

namespace Reni.Code
{
    /// <summary>
    /// Dereferencing operation
    /// </summary>
    [Serializable]
    internal sealed class Dereference : FiberItem
    {
        private readonly RefAlignParam _refAlignParam;
        private readonly Size _size;
        private readonly Size _dataSize;

        public Dereference(RefAlignParam refAlignParam, Size size, Size dataSize)
        {
            _refAlignParam = refAlignParam;
            _size = size;
            _dataSize = dataSize;
            StopByObjectId(-12);
        }

        [IsDumpEnabled(false)]
        internal RefAlignParam RefAlignParam { get { return _refAlignParam; } }
        [IsDumpEnabled(false)]
        internal Size DataSize { get { return _dataSize; } }

        [IsDumpEnabled(false)]
        public override string NodeDump { get { return base.NodeDump + " DataSize=" + DataSize; } }

        [IsDumpEnabled(false)]
        internal override Size InputSize { get { return RefAlignParam.RefSize; } }
        [IsDumpEnabled(false)]
        internal override Size OutputSize { get { return _size; } }

        internal override FiberItem[] TryToCombine(FiberItem subsequentElement)
        {
            return subsequentElement.TryToCombineBack(this);
        }

        internal override CodeBase TryToCombineBack(TopRef precedingElement)
        {
            Tracer.Assert(RefAlignParam.Equals(precedingElement.RefAlignParam));
            return new TopData(RefAlignParam, precedingElement.Offset, OutputSize, DataSize);
        }

        internal override CodeBase TryToCombineBack(FrameRef precedingElement)
        {
            Tracer.Assert(RefAlignParam.Equals(precedingElement.RefAlignParam));
            return new TopFrame(RefAlignParam, precedingElement.Offset, OutputSize, DataSize);
        }

        internal override void Execute(IFormalMaschine formalMaschine) { formalMaschine.Dereference(RefAlignParam, OutputSize, DataSize); }
    }
}