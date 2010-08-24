using HWClassLibrary.TreeStructure;
using System;
using HWClassLibrary.Helper;
using Reni.Code;
using Reni.Type;

namespace Reni.Context
{
    [Serializable]
    internal sealed class Function : Child, IReferenceInCode
    {
        [Node]
        internal readonly TypeBase ArgsType;

        internal Function(ContextBase parent, TypeBase argsType)
            : base(parent)
        {
            ArgsType = argsType;
        }

        internal override Result CreateArgsReferenceResult(Category category)
        {
            if(ArgsType is Reference)
            {
                return ArgsType.Result
                    (
                    category,
                    () => CodeBase.ReferenceInCode(this).Dereference(RefAlignParam, RefAlignParam.RefSize),
                    () => Refs.Create(this)
                    )
                    ;
            }

            return ArgsType.Reference(RefAlignParam).Result
                (
                category,
                () => CodeBase.ReferenceInCode(this),
                () => Refs.Create(this)
                )
                ;
        }

        RefAlignParam IReferenceInCode.RefAlignParam { get { return RefAlignParam; } }
        bool IReferenceInCode.IsChildOf(ContextBase contextBase) { return IsChildOf(contextBase); }
    }
}