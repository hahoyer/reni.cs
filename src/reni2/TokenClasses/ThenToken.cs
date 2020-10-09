using hw.Parser;
using Reni.Parser;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class ThenToken : InfixToken, IValueProvider, SyntaxFactory.IValueToken
    {
        public const string TokenId = "then";
        public override string Id => TokenId;

        SyntaxFactory.IValueProvider SyntaxFactory.IValueToken.Provider => SyntaxFactory.Then;
    }
}