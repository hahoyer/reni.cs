using hw.Parser;
using Reni.Parser;
using Reni.SyntaxFactory;

namespace Reni.TokenClasses;

[BelongsTo(typeof(MainTokenFactory))]
[UsedImplicitly]
sealed class Colon : TokenClass, IDeclarationToken
{
    public const string TokenId = ":";

    IStatementProvider IDeclarationToken.Provider => Factory.Colon;

    [DisableDump]
    public override string Id => TokenId;
}

[BelongsTo(typeof(MainTokenFactory))]
sealed class Exclamation : ParserTokenType<BinaryTree>, PrioParser<BinaryTree>.ISubParserProvider
{
    public const string TokenId = "!";

    readonly ISubParser<BinaryTree> Parser;

    public Exclamation(ISubParser<BinaryTree> parser) => Parser = parser;

    ISubParser<BinaryTree> PrioParser<BinaryTree>.ISubParserProvider.NextParser => Parser;

    [DisableDump]
    public override string Id => TokenId;

    protected override BinaryTree Create(BinaryTree? left, IToken token, BinaryTree? right)
    {
        NotImplementedMethod(left, token, right);
        return null!;
    }
}

[BelongsTo(typeof(DeclarationTokenFactory))]
abstract class DeclarationTagToken
    : TerminalToken, IDeclarationTag
{
    public static IEnumerable<string> DeclarationOptions
    {
        get
        {
            yield return ConverterToken.TokenId;
            yield return MutableAnnotation.TokenId;
            yield return MixInDeclarationToken.TokenId;
            yield return NonPositionalDeclarationToken.TokenId;
            yield return NonPublicDeclarationToken.TokenId;
            yield return PositionalDeclarationToken.TokenId;
            yield return PublicDeclarationToken.TokenId;
            yield return KernelPartDeclarationToken.TokenId;
            yield return NonKernelPartDeclarationToken.TokenId;
        }
    }
}

[UsedImplicitly]
sealed class ConverterToken : DeclarationTagToken
{
    internal const string TokenId = "converter";
    public override string Id => TokenId;
}

[UsedImplicitly]
sealed class MutableAnnotation : DeclarationTagToken
{
    internal const string TokenId = "mutable";

    [DisableDump]
    public override string Id => TokenId;
}

[UsedImplicitly]
sealed class MixInDeclarationToken : DeclarationTagToken
{
    internal const string TokenId = "mix_in";

    [DisableDump]
    public override string Id => TokenId;
}

[UsedImplicitly]
sealed class PublicDeclarationToken : DeclarationTagToken
{
    internal const string TokenId = "public";

    [DisableDump]
    public override string Id => TokenId;
}

[UsedImplicitly]
sealed class NonPublicDeclarationToken : DeclarationTagToken
{
    internal const string TokenId = "non_public";

    [DisableDump]
    public override string Id => TokenId;
}

[UsedImplicitly]
sealed class PositionalDeclarationToken : DeclarationTagToken
{
    internal const string TokenId = "positional";

    [DisableDump]
    public override string Id => TokenId;
}

[UsedImplicitly]
sealed class NonPositionalDeclarationToken : DeclarationTagToken
{
    internal const string TokenId = "non_positional";

    [DisableDump]
    public override string Id => TokenId;
}

[UsedImplicitly]
sealed class KernelPartDeclarationToken : DeclarationTagToken
{
    internal const string TokenId = "kernel_part";

    [DisableDump]
    public override string Id => TokenId;
}

[UsedImplicitly]
sealed class NonKernelPartDeclarationToken : DeclarationTagToken
{
    internal const string TokenId = "non_kernel_part";

    [DisableDump]
    public override string Id => TokenId;
}
