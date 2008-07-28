using System;
using HWClassLibrary.Debug;
using Reni.Context;

namespace Reni.Parser.TokenClass.Name
{
    [Token("this")]
    internal sealed class TthisT : Terminal
    {
        internal override Result Result(ContextBase context, Category category, Token token)
        {
            var structContext = context.FindStruct();
            return structContext.NaturalRefType
                .CreateContextResult(structContext.ForCode, category);
        }
    }
}