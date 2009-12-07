using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Context;
using Reni.Syntax;

namespace Reni.Parser.TokenClass.Name
{
    [Token("function")]
    [Serializable]
    internal sealed class Function : Prefix
    {
        public override Result Result(ContextBase context, Category category, ICompileSyntax right)
        {
            return context.CreateFunctionResult(category, right);
        }
    }
}