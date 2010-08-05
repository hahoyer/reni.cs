using HWClassLibrary.TreeStructure;
using System;
using HWClassLibrary.Helper;
using Reni.Code;
using Reni.Type;

namespace Reni.Context
{
    [Serializable]
    internal sealed class Function : Child, IRefInCode
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
                return ArgsType.CreateResult
                    (
                    category,
                    () => CodeBase.Create(this).CreateDereference(RefAlignParam, RefAlignParam.RefSize),
                    () => Refs.Create(this)
                    )
                    ;
            }

            return ArgsType.CreateReference(RefAlignParam).CreateResult
                (
                category,
                () => CodeBase.Create(this),
                () => Refs.Create(this)
                )
                ;
        }

        RefAlignParam IRefInCode.RefAlignParam { get { return RefAlignParam; } }
        bool IRefInCode.IsChildOf(ContextBase contextBase) { return IsChildOf(contextBase); }
    }
}