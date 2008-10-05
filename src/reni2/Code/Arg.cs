using System;
using HWClassLibrary.Debug;
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

        public override Result VirtVisit<Result>(Visitor<Result> actual)
        {
            return actual.Arg(this);
        }
    }

    internal class InternalRef : CodeBase
    {
        private readonly RefAlignParam _refAlignParam;
        private readonly IInternalResultProvider _internalProvider;

        public InternalRef(RefAlignParam refAlignParam, IInternalResultProvider internalProvider)
        {
            _refAlignParam = refAlignParam;
            _internalProvider = internalProvider;
        }

        internal protected override Size GetSize() { return _refAlignParam.RefSize; }

        public override Result VirtVisit<Result>(Visitor<Result> actual)
        {
            return actual.InternalRef(this);
        }
    }

}