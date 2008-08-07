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
    internal abstract class RefCode : LeafElement
    {
        private readonly RefAlignParam _refAlignParam;
        [Node]
        internal readonly Size Offset;

        protected RefCode(RefAlignParam refAlignParam, Size offset)
        {
            _refAlignParam = refAlignParam;
            Offset = offset;
        }

        protected override sealed Size GetSize()
        {
            return _refAlignParam.RefSize;
        }

        protected override Size GetInputSize()
        {
            return Size.Zero;
        }

        public override string NodeDump { get { return base.NodeDump + " Offset="+Offset; } }

        internal override RefAlignParam RefAlignParam { get { return _refAlignParam; } }
    }
}