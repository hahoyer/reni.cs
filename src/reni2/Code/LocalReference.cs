using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using Reni.Basics;
using Reni.Context;

namespace Reni.Code
{
    internal sealed class LocalReference : FiberHead, IReferenceInCode
    {
        private readonly RefAlignParam _refAlignParam;
        private static int _nextObjectId;

        [Node]
        private readonly CodeBase _unalignedCode;

        [Node, DisableDump]
        internal readonly CodeBase DestructorCode;

        public LocalReference(RefAlignParam refAlignParam, CodeBase code, CodeBase destructorCode)
            : base(_nextObjectId++)
        {
            _refAlignParam = refAlignParam;
            _unalignedCode = code;
            DestructorCode = destructorCode;
            StopByObjectId(-10);
        }

        RefAlignParam IReferenceInCode.RefAlignParam { get { return RefAlignParam; } }
        public bool IsChildOf(ContextBase contextBase) { return false; }

        [DisableDump]
        internal override bool IsRelativeReference { get { return false; } }

        [DisableDump]
        internal override RefAlignParam RefAlignParam { get { return _refAlignParam; } }

        internal CodeBase Code { get { return _unalignedCode.Align(RefAlignParam.AlignBits); } }

        protected override Size GetSize() { return _refAlignParam.RefSize; }
        protected override TResult VisitImplementation<TResult>(Visitor<TResult> actual) { return actual.LocalReference(this); }

        internal CodeBase AccompayningDestructorCode(ref Size size, string holder)
        {
            size += Code.Size;
            return DestructorCode.ReplaceArg(LocalVariableReference(RefAlignParam, holder),null);
        }
    }
}