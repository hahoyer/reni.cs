using Reni.Parser;

namespace Reni.TokenClasses.Whitespace;

sealed class RootType : VariantListType
{
    internal static readonly RootType Instance = new();

    [DisableDump]
    protected override ItemPrototype[] VariantPrototypes { get; } =
    {
        new(SpaceType.Instance, Lexer.Instance.Space)
        , new(LineEndType.Instance, Lexer.Instance.LineEnd)
        , new(LineCommentType.Instance, Lexer.Instance.LineComment)
        , new(InlineCommentType.Instance, Lexer.Instance.InlineComment),
    };
}