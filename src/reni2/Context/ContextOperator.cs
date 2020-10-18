using hw.Parser;
using Reni.Basics;
using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.Context
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class ContextOperator : TerminalSyntaxToken
    {
        public const string TokenId = "^^";
        public override string Id => TokenId;

        protected override Result Result
            (ContextBase context, Category category, TerminalSyntax token)
            => context
                .FindRecentCompoundView
                .ContextOperatorResult(category);
    }
}