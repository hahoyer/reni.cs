using hw.Parser;
using Reni.Parser;

namespace Reni.TokenClasses;

[BelongsTo(typeof(MainTokenFactory))]
sealed class ForeignCode : Definable
{
    public const string TokenId = "|\\*^°";
    public override string Id => TokenId;
}
