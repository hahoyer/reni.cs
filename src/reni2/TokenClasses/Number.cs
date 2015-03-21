﻿using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using Reni.Basics;
using Reni.Context;
using Reni.ReniParser;
using Reni.ReniSyntax;
using Reni.Validation;

namespace Reni.TokenClasses
{
    sealed class Number : TerminalToken
    {
        public override Result Result(ContextBase context, Category category, TerminalSyntax token)
            => context.RootContext.BitType.Result(category, BitsConst.Convert(token.Id));

        protected override Syntax Infix(Syntax left, IToken token, Syntax right)
            => new CompileSyntaxError(IssueId.UnexpectedUseAsSuffix, token.Characters);
        public override string Id => "<number>";
    }
}