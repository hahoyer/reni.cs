using System;
using HWClassLibrary.Debug;
using Reni.Context;

namespace Reni.Code
{
    /// <summary>
    /// Dereferencing operation
    /// </summary>
    [Serializable]
    internal sealed class Dereference : LeafElement
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
        internal override RefAlignParam RefAlignParam { get { return _refAlignParam; } }
        [IsDumpEnabled(false)]
        internal Size DataSize { get { return _dataSize; } }

        protected override Size GetSize()
        {
            return _size;
        }

        [IsDumpEnabled(false)]
        public override string NodeDump { get { return base.NodeDump + " DataSize=" + _dataSize; } }

        protected override Size GetInputSize()
        {
            return RefAlignParam.RefSize;
        }

        protected override string Format(StorageDescriptor start)
        {
            return start.CreateUnref(RefAlignParam, _size, _dataSize);
        }

        internal override LeafElement[] TryToCombineN(LeafElement subsequentElement)
        {
            return subsequentElement.TryToCombineBackN(this);
        }

        internal override LeafElement TryToCombineBack(TopRef precedingElement)
        {
            Tracer.Assert(RefAlignParam.Equals(precedingElement.RefAlignParam));
            return new TopData(RefAlignParam, precedingElement.Offset, GetSize(), DataSize);
        }

        internal override LeafElement TryToCombineBack(FrameRef precedingElement)
        {
            Tracer.Assert(RefAlignParam.Equals(precedingElement.RefAlignParam));
            return new TopFrame(RefAlignParam, precedingElement.Offset, GetSize(), DataSize);
        }

        internal override void Execute(IFormalMaschine formalMaschine) { formalMaschine.Dereference(RefAlignParam, Size, _dataSize); }
    }
}