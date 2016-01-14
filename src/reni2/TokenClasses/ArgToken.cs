using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Basics;
using Reni.Context;
using Reni.Formatting;
using Reni.Parser;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class ArgToken : NonSuffixSyntaxToken, IChainLink
    {
        public const string TokenId = "^";
        public override string Id => TokenId;

        protected override Result Result(ContextBase context, Category category, TerminalSyntax token)
            => context.ArgReferenceResult(category);

        internal override CompileSyntax Visit(ISyntaxVisitor visitor) => visitor.Arg;

        protected override Result Result
            (ContextBase context, Category category, PrefixSyntax token, CompileSyntax right)
            => context.FunctionalArgResult(category, right,token.SourcePart);
    }
}