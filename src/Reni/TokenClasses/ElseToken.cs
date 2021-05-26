using hw.Parser;
using Reni.Parser;
using Reni.SyntaxFactory;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class ElseToken : TokenClass, IValueToken
    {
        public const string TokenId = "else";
        public override string Id => TokenId;

        IValueProvider IValueToken.Provider => Factory.Else;
    }
}