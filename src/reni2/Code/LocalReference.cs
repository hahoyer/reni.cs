using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using Reni.Context;

namespace Reni.Code
{
    internal sealed class LocalReference : FiberHead, IReferenceInCode
    {
        private readonly RefAlignParam _refAlignParam;

        [Node]
        private readonly CodeBase _unalignedCode;

        [Node]
        internal readonly CodeBase DestructorCode;

        public LocalReference(RefAlignParam refAlignParam, CodeBase code, CodeBase destructorCode)
        {
            _refAlignParam = refAlignParam;
            _unalignedCode = code;
            DestructorCode = destructorCode;
            StopByObjectId(-1100);
        }

        RefAlignParam IReferenceInCode.RefAlignParam { get { return RefAlignParam; } }
        bool IReferenceInCode.IsChildOf(ContextBase contextBase) { return false; }

        protected override Size GetSize() { return _refAlignParam.RefSize; }
        protected override TResult VisitImplementation<TResult>(Visitor<TResult> actual) { return actual.LocalReference(this); }
        protected override bool IsRelativeReference { get { return false; } }

        internal override RefAlignParam RefAlignParam { get { return _refAlignParam; } }

        internal CodeBase Code { get { return _unalignedCode.Align(RefAlignParam.AlignBits); } }

        internal CodeBase AccompayningDestructorCode(ref Size size)
        {
            size += Code.Size;
            return DestructorCode.ReplaceArg(LocalReferenceCode(RefAlignParam, size, "AccompayningDestructorCode"));
        }

        [IsDumpEnabled(false)]
        public ContextRef ToLeafElement { get { return new ContextRef(this); } }


    }
}