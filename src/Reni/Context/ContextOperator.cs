using hw.Parser;
using hw.Scanner;
using Reni.Basics;
using Reni.DeclarationOptions;
using Reni.Parser;
using Reni.TokenClasses;

namespace Reni.Context;

[BelongsTo(typeof(MainTokenFactory))]
sealed class ContextOperator : TerminalSyntaxToken
{
    public const string TokenId = "^^";
    protected override Declaration[] Declarations => throw new NotImplementedException();
    public override string Id => TokenId;

    protected override Result GetResult(ContextBase context, Category category, SourcePart token)
        => context
            .FindRecentCompoundView
            .ContextOperatorResult(category);
}