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
    internal abstract class Ref : FiberHead
    {
        private readonly RefAlignParam _refAlignParam;
        [IsDumpEnabled(false)]
        internal readonly string Reason;
        [Node, IsDumpEnabled(false)]
        internal readonly Size Offset;

        protected Ref(RefAlignParam refAlignParam, Size offset, string reason)
        {
            _refAlignParam = refAlignParam;
            Reason = reason;
            Offset = offset;
        }

        protected override sealed Size GetSize() { return _refAlignParam.RefSize; }
        [IsDumpEnabled(false)]
        public override string NodeDump { get { return base.NodeDump + " Offset=" + Offset + " Reason=" + Reason; } }
        [IsDumpEnabled(false)]
        internal override RefAlignParam RefAlignParam { get { return _refAlignParam; } }
    }
}