using HWClassLibrary.Debug;
using System;
using HWClassLibrary.Helper;
using Reni.Context;

namespace Reni.Code
{
    internal class InternalRef : CodeBase, IRefInCode
    {
        private readonly RefAlignParam _refAlignParam;
        internal readonly CodeBase Code;
        internal readonly CodeBase DestructorCode;

        public InternalRef(RefAlignParam refAlignParam, CodeBase code, CodeBase destructorCode)
        {
            _refAlignParam = refAlignParam;
            Code = code;
            DestructorCode = destructorCode;
            StopByObjectId(-1100);
        }

        [DumpData(false)]
        public LeafElement ToLeafElement { get { return new ContextRef(this); } }

        protected override Size SizeImplementation { get { return _refAlignParam.RefSize; } }
        protected override Result VisitImplementation<Result>(Visitor<Result> actual) { return actual.InternalRef(this); }
        internal override RefAlignParam RefAlignParam { get { return _refAlignParam; } }

        Size IRefInCode.RefSize { get { return RefAlignParam.RefSize; } }
        RefAlignParam IRefInCode.RefAlignParam { get { return RefAlignParam; } }
        bool IRefInCode.IsChildOf(ContextBase contextBase) { return false; }

        internal CodeBase AccompayningDestructorCode(Size size)
        {
            size += Code.Size;
            return DestructorCode.UseWithArg(InternalRefSequenceVisitor.InternalRefCode(RefAlignParam, size));
        }
    }
}