using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Context;

namespace Reni.Code
{
    /// <summary>
    /// Arg is is used as a placeholder.
    /// </summary>
    internal sealed class Arg : CodeBase
    {
        private readonly Size _size;

        internal Arg(Size size)
        {
            _size = size;
            StopByObjectId(-579);
        }

        internal protected override Size GetSize() { return _size; }

        public override Result VirtVisit<Result>(Visitor<Result> actual) { return actual.Arg(this); }
    }

    internal class InternalRef : CodeBase, IContextRefInCode
    {
        private readonly RefAlignParam _refAlignParam;
        [Node]
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

        override internal CodeBase CreateStatement()
        {
            NotImplementedMethod();
            return null;
        }
    }
}