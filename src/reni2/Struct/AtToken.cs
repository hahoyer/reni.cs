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
            var leftType = callContext.Type(left);
            var atTargetType = leftType.GetAtTarget();
            var position = callContext.Evaluate(right, atTargetType.IndexType).ToInt32();

            NotImplementedMethod(callContext,category,left,right,"position",position);
            return null;
            //return callContext.AccessResult(category, left, position);
        }
    }
}