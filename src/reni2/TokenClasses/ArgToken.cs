using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.ReniParser;
using Reni.ReniSyntax;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class ArgToken : NonSuffix
    {
        public const string TokenId = "^";
        public override string Id => TokenId;

        public override Result Result(ContextBase context, Category category, TerminalSyntax token)
            => context.ArgReferenceResult(category);

        internal override CompileSyntax Visit(ISyntaxVisitor visitor) => visitor.Arg;

        public override Result Result(ContextBase context, Category category, PrefixSyntax token, CompileSyntax right)
            => context.FunctionalArgResult(category, right);
    }
}