using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Context;

namespace Reni.Code
{
    /// <summary>
    ///     Dereferencing operation
    /// </summary>
    [Serializable]
    internal sealed class Dereference : FiberItem
    {
        private readonly RefAlignParam _refAlignParam;
        private readonly Size _outputSize;
        private readonly Size _dataSize;
        private static int _nextObjectId;

        public Dereference(RefAlignParam refAlignParam, Size outputSize, Size dataSize)
            : base(_nextObjectId++)
        {
            _refAlignParam = refAlignParam;
            _outputSize = outputSize;
            _dataSize = dataSize;
            StopByObjectId(-28);
        }

        [DisableDump]
        internal override RefAlignParam RefAlignParam { get { return _refAlignParam; } }

        [DisableDump]
        internal Size DataSize { get { return _dataSize; } }

        [DisableDump]
        public override string NodeDump { get { return base.NodeDump + " DataSize=" + DataSize; } }

        [DisableDump]
        internal override Size InputSize { get { return RefAlignParam.RefSize; } }

        [DisableDump]
        internal override Size OutputSize { get { return _outputSize; } }

        protected override FiberItem[] TryToCombineImplementation(FiberItem subsequentElement) { return subsequentElement.TryToCombineBack(this); }

        internal override CodeBase TryToCombineBack(TopRef precedingElement)
        {
            Tracer.Assert(RefAlignParam.Equals(precedingElement.RefAlignParam));
            return new TopData(RefAlignParam, precedingElement.Offset, OutputSize, DataSize);
        }

        internal override CodeBase TryToCombineBack(LocalVariableReference precedingElement)
        {
            Tracer.Assert(RefAlignParam.Equals(precedingElement.RefAlignParam));
            return new LocalVariableAccess(RefAlignParam, precedingElement.Holder, precedingElement.Offset, OutputSize, _dataSize);
        }

        internal override CodeBase TryToCombineBack(TopFrameRef precedingElement)
        {
            Tracer.Assert(RefAlignParam.Equals(precedingElement.RefAlignParam));
            return new TopFrameData(RefAlignParam, precedingElement.Offset, OutputSize, DataSize);
        }

        protected override string CSharpCodeSnippet(Size top) { return CSharpGenerator.Dereference(top, InputSize, OutputSize); }

        protected override void Execute(IFormalMaschine formalMaschine) { formalMaschine.Dereference(RefAlignParam, OutputSize, DataSize); }
    }
}