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

        internal EnableCut EnableCut => Parent.Cache.EnableCut.Value;
        internal CodeBase ArgumentCode => Parent.GetArgumentCode();
        internal TypeBase Pointer => ForcedReference.Type();
        internal IReference ForcedReference => Parent.Cache.ForcedReference.Value;
        internal TypeBase SmartPointer => Parent.GetIsHollow()? Parent : Pointer;
        internal TypeBase FunctionInstance => Parent.Cache.FunctionInstanceType.Value;
        internal PointerType ForcedPointer => Parent.Cache.Pointer.Value;
        internal TypeBase TypeForStructureElement => Parent.GetDeAlign(Category.Type).Type;
        internal TypeBase TypeForArrayElement => Parent.GetDeAlign(Category.Type).Type;
        internal IReference? CheckedReference => Parent as IReference;
        internal TypeBase TypeType => Parent.GetTypeType();
        internal TypeBase TypeForTypeOperator => Parent.GetTypeForTypeOperator();
        internal IGenericProviderForType[] GenericProvidersForType => Parent.GetGenericProviders().ToArray();
        internal TypeBase TagTargetType => Parent.GetTagTargetType();

        internal TypeBase Align
        {
            get
            {
                var alignBits = Root.DefaultRefAlignParam.AlignBits;
                return Parent.OverView.Size.GetAlign(alignBits) == Parent.OverView.Size
                    ? Parent
                    : Parent.Cache.Aligner[alignBits];
            }
        }


        internal TypeBase AutomaticDereferenceType
            =>
                Parent.OverView.IsWeakReference
                    ? Parent.Make.CheckedReference!.Converter.ResultType().Make.AutomaticDereferenceType
                    : Parent;
    }
}
