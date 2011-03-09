using System;
using Reni.Context;
using Reni.Parser;
using Reni.ReniParser.TokenClasses;

namespace Reni.ReniParser.TokenClasses
{
    [Serializable]
    internal sealed class TthisT : Terminal
    {
        public override Result Result(ContextBase context, Category category, TokenData token)
        {
            return context.FindStruct().ThisReferenceResult(category);
        }
    }
}