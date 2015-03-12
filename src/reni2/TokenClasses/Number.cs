using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using Reni.Basics;
using Reni.Context;
using Reni.ReniParser;
using Reni.Validation;

namespace Reni.TokenClasses
{
    sealed class Number : TerminalToken
    {
        public override Result Result(ContextBase context, Category category, IToken token)
            => context.RootContext.BitType.Result(category, BitsConst.Convert(token.Id));

        public static Int64 ToInt64(IToken token) => BitsConst.Convert(token.Id).ToInt64();

        protected override Syntax Infix(Syntax left, IToken token, Syntax right)
            => new CompileSyntaxError(IssueId.UnexpectedUseAsSuffix, token);
        public override string Id => "<number>";
    }
}