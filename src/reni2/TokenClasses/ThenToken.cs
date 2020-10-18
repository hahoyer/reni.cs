using hw.Parser;
using Reni.Parser;
using Reni.SyntaxFactory;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class ThenToken : InfixToken, IValueToken
    {
        public const string TokenId = "then";
        public override string Id => TokenId;

        IValueProvider IValueToken.Provider => Factory.Then;
    }
}