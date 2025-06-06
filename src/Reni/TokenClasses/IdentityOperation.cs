using hw.Parser;
using Reni.Feature;
using Reni.Numeric;
using Reni.Parser;

namespace Reni.TokenClasses;

[BelongsTo(typeof(MainTokenFactory))]
[Variant(false)]
[Variant(true)]
sealed class IdentityOperation : Operation
{
    internal readonly bool IsEqual;

    public IdentityOperation(bool isEqual) => IsEqual = isEqual;

    public override string Id => TokenId(IsEqual);

    [DisableDump]
    internal override IEnumerable<IDeclarationProvider> MakeGeneric
        => this.GenericListFromDefinable(base.MakeGeneric);

    public static string TokenId(bool isEqual) => isEqual? "==" : "~==";
}