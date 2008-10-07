using HWClassLibrary.Debug;
using System;
using Reni.Context;

namespace Reni.Code
{
    internal class InternalRef : CodeBase, IContextRefInCode
    {
        private readonly RefAlignParam _refAlignParam;
        internal readonly IInternalResultProvider InternalProvider;

        public InternalRef(RefAlignParam refAlignParam, IInternalResultProvider internalProvider)
        {
            _refAlignParam = refAlignParam;
            InternalProvider = internalProvider;
        }

        [DumpData(false)]
        public LeafElement ToLeafElement { get { return new ContextRef(this); } }

        internal protected override Size GetSize() { return _refAlignParam.RefSize; }

        public override Result VirtVisit<Result>(Visitor<Result> actual) { return actual.InternalRef(this); }
        internal override RefAlignParam RefAlignParam { get { return _refAlignParam; } }

        Size IContextRefInCode.RefSize { get { return RefAlignParam.RefSize; } }
        RefAlignParam IContextRefInCode.RefAlignParam { get { return RefAlignParam; } }
        bool IContextRefInCode.IsChildOf(ContextBase contextBase) { return false; }

    }
}