using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Context;
using Reni.Parser;
using Reni.TokenClasses;
using Reni.Syntax;

namespace Reni.Struct
{
    internal sealed class AtToken : Infix
    {
        public override Result Result(ContextBase callContext, Category category, ICompileSyntax left,
                                      ICompileSyntax right)
        {
            var trace = left != null && left.GetObjectId() == -24 && category.HasType;
            StartMethodDumpWithBreak(trace, callContext, category, left, right);

            var typeBase = callContext.Type(left);
            var context = typeBase.GetStruct();
            var position = callContext.Evaluate(right, context.IndexType).ToInt32();
            var atResult = context.AccessResultFromArg(category, position);

            var leftResult = callContext.ResultAsRef(category, left);
            DumpWithBreak(trace, "atResult", atResult, "leftResult", leftResult);
            return ReturnMethodDumpWithBreak(trace, atResult.ReplaceArg(leftResult));
        }
    }
}