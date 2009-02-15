using System;
using HWClassLibrary.Debug;
using Reni.Context;
using Reni.Struct;
using Reni.Syntax;

namespace Reni.Parser.TokenClass.Name
{
    [Token("_A_T_")]
    [Serializable]
    internal sealed class T_A_T_T : Infix
    {
        internal override Result Result(ContextBase callContext, Category category, ICompileSyntax left, Token token,
                                        ICompileSyntax right)
        {
            var position = callContext.Evaluate(right, callContext.Type(left).IndexType).ToInt32();
            return PositionFeatureBase.AccessResult(callContext, category, left, position);
        }
    }
}