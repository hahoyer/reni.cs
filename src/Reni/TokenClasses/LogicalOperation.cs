using hw.Parser;
using Reni.Numeric;
using Reni.Parser;

namespace Reni.TokenClasses;

[BelongsTo(typeof(MainTokenFactory))]
sealed class NotOperation : Operation
{
    public static readonly string TokenId = "~";
    public override string Id => TokenId;
}

[BelongsTo(typeof(MainTokenFactory))]
sealed class AndOperation : Operation
{
    public static readonly string TokenId = "&";
    public override string Id => TokenId;
}

[BelongsTo(typeof(MainTokenFactory))]
sealed class OrOperation : Operation
{
    public static readonly string TokenId = "|";
    public override string Id => TokenId;
}