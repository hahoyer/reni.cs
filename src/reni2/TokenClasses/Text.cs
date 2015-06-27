using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Formatting;
using Reni.Parser;

namespace Reni.TokenClasses
{
    sealed class Text : TerminalSyntaxToken, IChainLink
    {
        public override Result Result(ContextBase context, Category category, TerminalSyntax token)
        {
            var data = StripQutes(token.Id);
            return context
                .RootContext.BitType.Array(BitsConst.BitSize(data[0].GetType()))
                .TextItem
                .Array(data.Length)
                .TextItem
                .Result
                (category, () => CodeBase.BitsConst(BitsConst.ConvertAsText(data)), CodeArgs.Void);
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
        public override string Id => "<text>";
    }
}