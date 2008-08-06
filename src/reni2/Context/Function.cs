using System;
using HWClassLibrary.Helper;
using Reni.Code;
using Reni.Type;

namespace Reni.Context
{
    [Serializable]
    internal sealed class Function : Child, IContextRefInCode
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
            return ArgsType.CreateAutomaticRef(RefAlignParam).CreateResult
                (
                category,
                () => CodeBase.CreateContextRef(this),
                () => Refs.Context(this)
                )
                ;
        }

        RefAlignParam IContextRefInCode.RefAlignParam { get { return RefAlignParam; } }
        bool IContextRefInCode.IsChildOf(ContextBase contextBase) { return IsChildOf(contextBase); }
    }
}