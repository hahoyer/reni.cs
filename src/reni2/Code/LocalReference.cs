using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Forms;
using Reni.Basics;
using Reni.Context;

namespace Reni.Code
{
    sealed class LocalReference : FiberHead
    {
        static int _nextObjectId;

        [Node]
        readonly CodeBase _unalignedCode;

        [Node]
        [DisableDump]
        internal readonly CodeBase DestructorCode;

        public LocalReference(CodeBase code, CodeBase destructorCode)
            : base(_nextObjectId++)
        {
            _unalignedCode = code;
            DestructorCode = destructorCode;
            StopByObjectId(-8);
        }

        [DisableDump]
        internal override bool IsRelativeReference { get { return false; } }

        internal CodeBase Code { get { return _unalignedCode.Align(); } }

        protected override Size GetSize() { return Root.DefaultRefAlignParam.RefSize; }
        protected override CodeArgs GetRefsImplementation() { return _unalignedCode.Exts + DestructorCode.Exts; }
        protected override TResult VisitImplementation<TResult>(Visitor<TResult> actual) { return actual.LocalReference(this); }

        internal CodeBase AccompayningDestructorCode(ref Size size, string holder)
        {
            size += Code.Size;
            return DestructorCode.ReplaceArg(null, LocalVariableReference(holder));
        }
    }
}