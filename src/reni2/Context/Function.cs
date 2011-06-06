using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using Reni.Type;

namespace Reni.Context
{
    [Serializable]
    internal sealed class Function : Child
    {
        [Node]
        internal readonly TypeBase ArgsType;

        internal Function(TypeBase argsType) { ArgsType = argsType; }

        protected override Result CreateArgsReferenceResult(ContextBase contextBase, Category category) { return ArgsType.ReferenceInCode(contextBase, category); }
    }
}