using hw.Parser;
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

        protected override Result Result(ContextBase context, Category category)
            => context.ArgReferenceResult(category);

        internal override Syntax Visit(ISyntaxVisitor visitor) => visitor.Arg;

        protected override Result Result
            (ContextBase context, Category category, Syntax right, BinaryTree token)
            => context.FunctionalArgResult(category, right, token);
    }
}