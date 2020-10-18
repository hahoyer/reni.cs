using Reni.Basics;
using Reni.Context;
using Reni.SyntaxTree;

namespace Reni.TokenClasses
{
    sealed class Number : TerminalSyntaxToken
    {
        public override string Id => "<number>";

        protected override Result Result(ContextBase context, Category category, TerminalSyntax token)
            => context.RootContext.BitType.Result(category, BitsConst.Convert(token.Id));
    }
}