using hw.Parser;
using Reni.Parser;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class ElseToken : TokenClass, IValueProvider, SyntaxFactory.IValueToken
    {
        public const string TokenId = "else";
        public override string Id => TokenId;

        SyntaxFactory.IValueProvider SyntaxFactory.IValueToken.Provider => SyntaxFactory.Else;
    }
}