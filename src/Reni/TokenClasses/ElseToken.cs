using hw.Parser;
using Reni.Parser;
using Reni.SyntaxFactory;

namespace Reni.TokenClasses;

[BelongsTo(typeof(MainTokenFactory))]
sealed class ElseToken : TokenClass, IValueToken, IBelongingsMatcher
{
    public const string TokenId = "else";

    bool IBelongingsMatcher.IsBelongingTo(IBelongingsMatcher otherMatcher)
        => otherMatcher is ThenToken;

    IValueProvider IValueToken.Provider => Factory.Else;

    public override string Id => TokenId;
}