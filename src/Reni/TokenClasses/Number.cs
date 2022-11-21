using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.DeclarationOptions;

namespace Reni.TokenClasses;

sealed class Number : TerminalSyntaxToken
{
    static readonly Declaration[] PredefinedDeclarations = { new("dumpprint") };

    protected override Declaration[] Declarations => PredefinedDeclarations;
    public override string Id => "<number>";

    protected override Result Result(ContextBase context, Category category, SourcePart token)
        => context.RootContext.BitType.Result(category, BitsConst.Convert(token.Id));
}