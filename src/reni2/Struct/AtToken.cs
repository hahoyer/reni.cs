using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Context;
using Reni.Parser.TokenClass;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Struct
{
    internal sealed class AtToken : Infix
    {
        public override Result Result(ContextBase callContext, Category category, ICompileSyntax left,
                                      ICompileSyntax right)
        {
            var typeBase = callContext.Type(left);
            var context = typeBase.GetStruct();
            var position = callContext.Evaluate(right, context.IndexType).ToInt32();
            var atResult = context.CreateAtResultFromArg(category, position);

            var leftResult = callContext.ResultAsRef(category, left);
            return atResult.UseWithArg(leftResult);
        }
    }
}