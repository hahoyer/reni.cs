using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Context;
using Reni.Parser.TokenClass;
using Reni.Syntax;

namespace Reni.Struct
{
    internal sealed class AtToken : Infix
    {
        public override Result Result(ContextBase callContext, Category category, ICompileSyntax left, ICompileSyntax right)
        {
            var position = callContext.Evaluate(right, callContext.Type(left).IndexType).ToInt32();
            return callContext.AccessResult(category, left, position);
        }
    }
}