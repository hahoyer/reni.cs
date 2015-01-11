using System.Linq;
using System.Collections.Generic;
using System;
using Reni.Basics;
using Reni.Context;
using Reni.ReniSyntax;

namespace Reni.TokenClasses
{
    sealed class FunctionInstanceToken : SuffixToken
    {
        public override Result Result(ContextBase context, Category category, CompileSyntax left)
        {
            var leftResult = left.Result(context, category.Typed);
            return leftResult.Type.FunctionInstance.Result(category, leftResult);
        }
    }
}