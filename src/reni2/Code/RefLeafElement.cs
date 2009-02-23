using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Context;

namespace Reni.Code
{
    /// <summary>
    /// Reference to something 
    /// </summary>
    [Serializable]
    internal abstract class RefLeafElement : LeafElement
    {
        private readonly RefAlignParam _refAlignParam;

        [Node]
        [DumpData(false)]
        internal readonly Size Offset;

        protected RefLeafElement(RefAlignParam refAlignParam, Size offset)
        {
            _refAlignParam = refAlignParam;
            Offset = offset;
        }

        protected override sealed Size GetSize() { return _refAlignParam.RefSize; }
        protected override Size GetInputSize() { return Size.Zero; }
        [DumpData(false)]
        public override string NodeDump { get { return base.NodeDump + " Offset=" + Offset; } }
        [DumpData(false)]
        internal override RefAlignParam RefAlignParam { get { return _refAlignParam; } }
    }
}