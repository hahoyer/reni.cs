using hw.Scanner;
using Reni.Basics;
using Reni.Context;

namespace Reni.TokenClasses
{
    sealed class Number : TerminalSyntaxToken
    {
        public override string Id => "<number>";

        protected override Result Result(ContextBase context, Category category, SourcePart token)
            => context.RootContext.BitType.Result(category, BitsConst.Convert(token.Id));
    }
}