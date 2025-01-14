using hw.Parser;
using Reni.Parser;
using Reni.SyntaxFactory;

namespace Reni.TokenClasses;

[BelongsTo(typeof(MainTokenFactory))]
sealed class Cleanup : TokenClass, IValueToken
{
    public const string TokenId = "~~~";
    public override string Id => TokenId;

    IValueProvider IValueToken.Provider => Factory.Cleanup;
}