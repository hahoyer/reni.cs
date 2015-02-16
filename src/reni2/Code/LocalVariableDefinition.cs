using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Forms;
using Reni.Basics;
using Reni.Type;

namespace Reni.Code
{
    sealed class LocalVariableDefinition : FiberItem
    {
        public LocalVariableDefinition(int id, int index, CodeBase destructorCode, TypeBase pointer)
        {
            Index = index;
            Id = id;
            DestructorCode = destructorCode;
            Pointer = pointer;
            StopByObjectId(-4);
        }

        [Node]
        int Id { get; }
        [Node]
        int Index { get; }
        [Node]
        [DisableDump]
        internal CodeBase DestructorCode { get; }
        [Node]
        public TypeBase Pointer { get; }
        [DisableDump]
        internal override Size InputSize => Pointer.AutomaticDereferenceType.Size;
        [DisableDump]
        internal override Size OutputSize => Size.Zero;
        [DisableDump]
        public string NameInCode => "lv_" + Id + "_" + Index;

        protected override string GetNodeDump()
            => base.GetNodeDump() + " NameInCode=" + NameInCode + " ValueSize=" + InputSize;
        internal override void Visit(IVisitor visitor)
            => visitor.LocalVariableDefinition(NameInCode, InputSize);

        internal CodeBase AccompayningDestructorCode(ref Size size, LocalReference definition)
        {
            size += InputSize;
            return DestructorCode.ReplaceArg(Pointer, CodeBase.LocalVariableReference(this));
        }
    }
}