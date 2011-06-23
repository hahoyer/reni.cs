using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using Reni.Context;

namespace Reni.Code
{
    /// <summary>
    ///     Reference to something
    /// </summary>
    [Serializable]
    internal abstract class Ref : FiberHead
    {
        private readonly RefAlignParam _refAlignParam;

        [Node, DisableDump]
        internal readonly Size Offset;

        protected Ref(RefAlignParam refAlignParam, Size offset)
        {
            _refAlignParam = refAlignParam;
            Offset = offset;
        }

        protected override sealed Size GetSize() { return _refAlignParam.RefSize; }

        [DisableDump]
        public override string NodeDump { get { return base.NodeDump + " Offset=" + Offset; } }

        [DisableDump]
        internal override RefAlignParam RefAlignParam { get { return _refAlignParam; } }
    }
}