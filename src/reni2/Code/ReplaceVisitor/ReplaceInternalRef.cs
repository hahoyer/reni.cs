using HWClassLibrary.Debug;
using System;
using Reni.Context;

namespace Reni.Code.ReplaceVisitor
{
    internal sealed class ReplaceInternalRef : Base
    {
        internal readonly Size Offset;
        private readonly RefAlignParam _refAlignParam;

        internal ReplaceInternalRef(Size offset, RefAlignParam refAlignParam)
        {
            _refAlignParam = refAlignParam;
            Offset = offset;
        }

        internal override CodeBase InternalRef(InternalRef visitedObject)
        {
            return CodeBase.CreateTopRef(_refAlignParam,Offset);
        }

        internal override Visitor<CodeBase> After(Size size)
        {
            if(size.IsZero)
                return this;
            return new ReplaceInternalRef(Offset + size, _refAlignParam);
        }
    }
}