using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.ReniParser;
using Reni.Validation;

namespace Reni.TokenClasses
{
    sealed class Number : TerminalToken
    {
        public override Result Result(ContextBase context, Category category, Token token)
            => context.RootContext.BitType.Result(category, BitsConst.Convert(token.Name));
        public static Int64 ToInt64(hw.Parser.Token token) => BitsConst.Convert(token.Name).ToInt64();
        protected override Syntax Infix(Syntax left, Token token, Syntax right)
            =>
                new CompileSyntaxError
                    (IssueId.UnexpectedUseAsSuffix, token, sourcePart: left.SourcePart + right.SourcePart);
    }
}