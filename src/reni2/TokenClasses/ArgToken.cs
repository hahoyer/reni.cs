using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Basics;
using Reni.Context;
using Reni.Parser;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class ArgToken : NonSuffixSyntaxToken
    {
        public const string TokenId = "^";
        public override string Id => TokenId;

        protected override Result Result(ContextBase context, Category category, TerminalSyntax token)
            => context.ArgReferenceResult(category);

        internal override Value Visit(ISyntaxVisitor visitor) => visitor.Arg;

        protected override Result Result
            (ContextBase context, Category category, PrefixSyntax token, Value right)
            => context.FunctionalArgResult(category, right,token.SourcePart);
    }
}