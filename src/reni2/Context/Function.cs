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

        internal override Result CreateArgsRefResult(Category category)
        {
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