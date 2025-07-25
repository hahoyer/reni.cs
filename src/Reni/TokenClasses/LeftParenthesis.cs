using hw.Parser;
using Reni.Parser;
using Reni.TokenClasses.Brackets;

namespace Reni.TokenClasses;

[BelongsTo(typeof(MainTokenFactory))]
[BelongsTo(typeof(DeclarationTokenFactory))]
[Variant(1)]
[Variant(2)]
[Variant(3)]
sealed class LeftParenthesis : TokenClass, IBelongingsMatcher, ILeftBracket
{
    internal int Level { get; }

    public LeftParenthesis(int level) => Level = level;

    bool IBelongingsMatcher.IsBelongingTo(IBelongingsMatcher otherMatcher)
        => (otherMatcher as RightParenthesis)?.Level == Level;

    Setup IBracket.Setup => Setup.Instances[Level];

    int IBracket.Level => Level;

    [DisableDump]
    public override string Id => ((IBracket)this).Setup.OpeningTokenId;

}
