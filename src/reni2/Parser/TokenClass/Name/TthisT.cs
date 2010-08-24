using System;
using Reni.Context;

namespace Reni.Parser.TokenClass.Name
{
    [Serializable]
    internal sealed class TthisT : Terminal
    {
        public override Result Result(ContextBase context, Category category, Token token)
        {
            return context.FindStruct().ThisReferenceResult(category);
        }
    }
}