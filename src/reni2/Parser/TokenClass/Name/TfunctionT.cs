using System;
using HWClassLibrary.Debug;
using Reni.Context;
using Reni.Syntax;

namespace Reni.Parser.TokenClass.Name
{
    [Token("function")]
    [Serializable]
    internal sealed class TfunctionT : Prefix
    {
        internal override Result Result(ContextBase context, Category category, ICompileSyntax right)
        {
            return context.CreateFunctionResult(category, right);
        }
    }
}