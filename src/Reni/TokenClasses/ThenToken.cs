using hw.Parser;
using Reni.Parser;
using Reni.SyntaxFactory;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class ThenToken : InfixToken, IValueToken, IBelongingsMatcher
    {
        public const string TokenId = "then";

        bool IBelongingsMatcher.IsBelongingTo(IBelongingsMatcher otherMatcher)
            => otherMatcher is ElseToken;

        IValueProvider IValueToken.Provider => Factory.Then;

        public override string Id => TokenId;
    }
}