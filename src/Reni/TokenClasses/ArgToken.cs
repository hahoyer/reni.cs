using hw.Parser;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.DeclarationOptions;
using Reni.Parser;
using Reni.SyntaxTree;

namespace Reni.TokenClasses;

[BelongsTo(typeof(MainTokenFactory))]
sealed class ArgToken : NonSuffixSyntaxToken
{
    public const string TokenId = "^";

    static readonly Declaration[] PredefinedDeclarations = { new("dumpprint") };
    public override string Id => TokenId;

    protected override Result Result(ContextBase context, Category category)
        => context.ArgReferenceResult(category);

    internal override ValueSyntax Visit(ISyntaxVisitor visitor) => visitor.Arg;

    protected override Declaration[] Declarations => PredefinedDeclarations;

    protected override Result Result(ContextBase context, Category category, ValueSyntax right, SourcePart token)
        => context.FunctionalArgResult(category, right, token);
}