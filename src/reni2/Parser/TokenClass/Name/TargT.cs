using System;
using HWClassLibrary.Debug;
using Reni.Context;

namespace Reni.Parser.TokenClass.Name
{
    [Token("arg")]
    [Serializable]
    internal sealed class TargT : Terminal
    {
        internal override Result Result(ContextBase context, Category category, Token token)
        {
            return context.CreateArgsRefResult(category);
        }
    }
}