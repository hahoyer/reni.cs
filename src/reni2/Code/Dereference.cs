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
        private readonly Size _outputSize;
        private readonly Size _dataSize;

        public Dereference(RefAlignParam refAlignParam, Size outputSize, Size dataSize)
        {
            _refAlignParam = refAlignParam;
            _outputSize = outputSize;
            _dataSize = dataSize;
            StopByObjectId(-325);
        }

        [IsDumpEnabled(false)]
        internal override RefAlignParam RefAlignParam { get { return _refAlignParam; } }
        [IsDumpEnabled(false)]
        internal Size DataSize { get { return _dataSize; } }

        [IsDumpEnabled(false)]
        public override string NodeDump { get { return base.NodeDump + " DataSize=" + DataSize; } }

        [IsDumpEnabled(false)]
        internal override Size InputSize { get { return RefAlignParam.RefSize; } }
        [IsDumpEnabled(false)]
        internal override Size OutputSize { get { return _outputSize; } }

        internal override FiberItem[] TryToCombine(FiberItem subsequentElement)
        {
            return subsequentElement.TryToCombineBack(this);
        }

        internal override CodeBase TryToCombineBack(TopRef precedingElement)
        {
            Tracer.Assert(RefAlignParam.Equals(precedingElement.RefAlignParam));
            return new TopData(RefAlignParam, precedingElement.Offset, OutputSize, DataSize);
        }

        internal override CodeBase TryToCombineBack(LocalVariableReference precedingElement)
        {
            return null;
            Tracer.Assert(RefAlignParam.Equals(precedingElement.RefAlignParam));
            return new LocalVariableAccess(RefAlignParam, precedingElement.Holder, precedingElement.Offset, OutputSize);
        }

        internal override CodeBase TryToCombineBack(FrameRef precedingElement)
        {
            Tracer.Assert(RefAlignParam.Equals(precedingElement.RefAlignParam));
            return new TopFrame(RefAlignParam, precedingElement.Offset, OutputSize, DataSize);
        }

        protected override string CSharpCodeSnippet(Size top) { return CSharpGenerator.Dereference(top, InputSize, OutputSize); }

        protected override void Execute(IFormalMaschine formalMaschine) { formalMaschine.Dereference(RefAlignParam, OutputSize, DataSize); }
    }
}