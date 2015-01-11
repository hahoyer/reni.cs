using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Forms;
using Reni.Basics;
using Reni.Context;
using Reni.Type;

namespace Reni.Code
{
    sealed class LocalReference : FiberHead
    {
        static int _nextObjectId;

        public LocalReference(TypeBase valueType, CodeBase valueCode, CodeBase destructorCode)
            : base(_nextObjectId++)
        {
            ValueType = valueType;
            ValueCode = valueCode;
            Tracer.Assert(valueCode.Size == ValueType.Size);
            DestructorCode = destructorCode;
            StopByObjectId(-8);
        }

        [Node]
        [DisableDump]
        internal CodeBase DestructorCode { get; }
        [Node]
        internal CodeBase ValueCode { get; }
        [Node]
        public TypeBase ValueType { get; }

        internal CodeBase Code => ValueCode.Align();

        protected override Size GetSize() => Root.DefaultRefAlignParam.RefSize;
        protected override CodeArgs GetRefsImplementation() => ValueCode.Exts + DestructorCode.Exts;
        protected override TResult VisitImplementation<TResult>(Visitor<TResult> actual) => actual.LocalReference(this);

        internal CodeBase AccompayningDestructorCode(ref Size size, string holder)
        {
            size += Code.Size;
            return DestructorCode.ReplaceArg(ValueType.Pointer, LocalVariableReference(holder));
        }
    }
}