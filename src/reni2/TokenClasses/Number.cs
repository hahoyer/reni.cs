using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Basics;
using Reni.Context;
using Reni.Formatting;
using Reni.Parser;

namespace Reni.TokenClasses
{
    sealed class Number : TerminalSyntaxToken, IChainLink
    {
        protected override Result Result(ContextBase context, Category category, TerminalSyntax token)
            => context.RootContext.BitType.Result(category, BitsConst.Convert(token.Id));

        public override string Id => "<number>";
    }
}