using System.Collections.Generic;
using System.Linq;
using System;
using hw.Parser;
using Reni.Basics;
using Reni.Code;
using Reni.Context;

namespace Reni.TokenClasses
{
    sealed class Text : TerminalToken
    {
        public override Result Result(ContextBase context, Category category, TokenData token)
        {
            var data = StripQutes(token.Name);
            return context
                .RootContext.BitType.UniqueArray(BitsConst.BitSize(data[0].GetType()))
                .UniqueTextItemType
                .UniqueArray(data.Length)
                .UniqueTextItemType
                .Result(category, () => CodeBase.BitsConst(BitsConst.ConvertAsText(data)), CodeArgs.Void);
        }

        static string StripQutes(string text)
        {
            var result = "";
            for(var i = 1; i < text.Length - 1; i++)
            {
                result += text[i];
                if(text[i] == text[0])
                    i++;
            }
            return result;
        }
    }
}