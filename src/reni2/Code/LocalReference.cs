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

        public LocalReference
            (TypeBase valueType, CodeBase valueCode, CodeBase destructorCode, bool isUsedOnce)
            : base(_nextObjectId++)
        {
            ValueType = valueType;
            ValueCode = valueCode;
            Tracer.Assert(valueCode.Size == ValueType.Size);
            DestructorCode = destructorCode;
            IsUsedOnce = isUsedOnce;
            StopByObjectIds();
        }

        [Node]
        [DisableDump]
        internal CodeBase DestructorCode { get; }
        [Node]
        [EnableDumpExcept(false)]
        bool IsUsedOnce { get; }
        [Node]
        internal CodeBase ValueCode { get; }
        [Node]
        public TypeBase ValueType { get; }
        [DisableDump]
        internal CodeBase AlignedValueCode => ValueCode.Align();

        protected override Size GetSize() => Root.DefaultRefAlignParam.RefSize;
        protected override CodeArgs GetRefsImplementation() => ValueCode.Exts + DestructorCode.Exts;
        protected override TCode VisitImplementation<TCode, TFiber>(Visitor<TCode, TFiber> actual)
            => actual.LocalReference(this);

        protected override CodeBase TryToCombine(FiberItem subsequentElement)
            => IsUsedOnce ? subsequentElement.TryToCombineBack(this) : null;

        internal CodeBase AccompayningDestructorCode
            (ref Size size, LocalReference definition)
        {
            size += AlignedValueCode.Size;
            return DestructorCode.ReplaceArg(ValueType.Pointer, definition);
        }
    }
}