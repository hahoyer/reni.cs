using hw.Parser;
using Reni.Parser;
using Reni.Struct;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class Cleanup : TokenClass, IValueProvider, SyntaxFactory.IValueToken
    {
        public const string TokenId = "~~~";
        public override string Id => TokenId;

        SyntaxFactory.IValueProvider SyntaxFactory.IValueToken.Provider => SyntaxFactory.Cleanup;
    }
}