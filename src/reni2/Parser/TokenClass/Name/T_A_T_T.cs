using System;
using HWClassLibrary.Debug;
using Reni.Context;
using Reni.Syntax;

namespace Reni.Parser.TokenClass.Name
{
    [Token("_A_T_")]
    [Serializable]
    internal sealed class T_A_T_T : Infix
    {
        internal override Result Result(ContextBase callContext, Category category, ICompileSyntax left, Token token, ICompileSyntax right)
        {
            var objectResult = callContext.ResultAsRef(category | Category.Type, left);
            var position = callContext.Evaluate(right, callContext.Type(left).IndexType).ToInt32();
            return objectResult.Type.AccessResult(category, position).UseWithArg(objectResult);
        }
    }
}