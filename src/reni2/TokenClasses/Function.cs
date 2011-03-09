using HWClassLibrary.Debug;
using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Context;
using Reni.Syntax;

namespace Reni.TokenClasses
{
    [Serializable]
    internal sealed class Function : Suffix
    {
        public override Result Result(ContextBase context, Category category, ICompileSyntax target) { return context.FindStruct().FunctionalResult(category, target); }
    }
}