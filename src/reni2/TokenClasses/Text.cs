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
        protected override Result Result(ContextBase context, Category category, TerminalSyntax token)
        {
            var data = Lexer.Instance.ExtractText(token.Token);
            return context
                .RootContext.BitType.Array(BitsConst.BitSize(data[0].GetType()))
                .TextItem
                .Array(data.Length)
                .TextItem
                .Result
                (category, () => CodeBase.BitsConst(BitsConst.ConvertAsText(data)), CodeArgs.Void);
        }

        public override string Id => "<text>";
    }
}