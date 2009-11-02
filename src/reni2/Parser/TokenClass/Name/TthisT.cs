using System;
using HWClassLibrary.Debug;
using Reni.Context;

namespace Reni.Parser.TokenClass.Name
{
    [Token("this")]
    [Serializable]
    internal sealed class TthisT : Terminal
    {
        public override Result Result(ContextBase context, Category category, Token token)
        {
            var structContext = context.FindStruct();
            return structContext.NaturalRefType
                .CreateContextResult(structContext.ForCode, category);
        }
    }
}