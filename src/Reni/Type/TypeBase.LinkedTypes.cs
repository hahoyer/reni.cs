using System.Diagnostics;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;

namespace Reni.Type;

abstract partial class TypeBase
{
    internal sealed class LinkedTypes(TypeBase parent)
    {
        [DisableDump]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        readonly TypeBase Parent = parent;

        [DisableDump]
        internal EnableCut EnableCut => Parent.Cache.EnableCut.Value;

        [DisableDump]
        internal TypeBase Align
        {
            get
            {
                var alignBits = Root.DefaultRefAlignParam.AlignBits;
                return Parent.Size.GetAlign(alignBits) == Parent.Size? Parent : Parent.Cache.Aligner[alignBits];
            }
        }

        [DisableDump]
        internal CodeBase ArgumentCode => Parent.GetArgumentCode();

        [DisableDump]
        internal TypeBase AutomaticDereferenceType
            =>
                Parent.IsWeakReference
                    ? Parent.Make.CheckedReference!.Converter.ResultType().Make.AutomaticDereferenceType
                    : Parent;

        [DisableDump]
        internal TypeBase Pointer => ForcedReference.Type();

        [DisableDump]
        internal IReference ForcedReference => Parent.Cache.ForcedReference.Value;

        [DisableDump]
        internal TypeBase SmartPointer => Parent.IsHollow? Parent : Pointer;

        [DisableDump]
        internal TypeBase FunctionInstance => Parent.Cache.FunctionInstanceType.Value;

        [DisableDump]
        internal PointerType ForcedPointer => Parent.Cache.Pointer.Value;
        [DisableDump]
        internal TypeBase TypeForStructureElement => Parent.GetDeAlign(Category.Type).Type;

        [DisableDump]
        internal TypeBase TypeForArrayElement => Parent.GetDeAlign(Category.Type).Type;

        [DisableDump]
        internal IReference? CheckedReference => Parent as IReference;

        [DisableDump]
        internal TypeBase TypeType => Parent.GetTypeType();

        [DisableDump]
        internal TypeBase TypeForTypeOperator => Parent.GetTypeForTypeOperator();

    }
}
