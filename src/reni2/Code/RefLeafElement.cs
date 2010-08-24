using HWClassLibrary.TreeStructure;
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
    internal abstract class RefLeafElement : StartingLeafElement
    {
        private readonly RefAlignParam _refAlignParam;

        [Node]
        [IsDumpEnabled(false)]
        internal readonly Size Offset;

        protected RefLeafElement(RefAlignParam refAlignParam, Size offset)
        {
            _refAlignParam = refAlignParam;
            Offset = offset;
        }

        protected override sealed Size GetSize() { return _refAlignParam.RefSize; }
        [IsDumpEnabled(false)]
        public override string NodeDump { get { return base.NodeDump + " Offset=" + Offset; } }
        [IsDumpEnabled(false)]
        internal override RefAlignParam RefAlignParam { get { return _refAlignParam; } }
    }
}