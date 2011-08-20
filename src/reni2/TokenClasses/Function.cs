using HWClassLibrary.Debug;
using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Basics;
using Reni.Context;
using Reni.Syntax;

namespace Reni.TokenClasses
{
    [Serializable]
    internal sealed class Function : Suffix
    {
        public override Result Result(ContextBase context, Category category, CompileSyntax target)
        {
            return context
                .FindRecentStructure.UniqueFunctionalFeature(target)
                .Result(category);
        }
    }
}