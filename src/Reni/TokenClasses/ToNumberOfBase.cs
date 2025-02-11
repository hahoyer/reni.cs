using hw.Parser;
using Reni.Feature;
using Reni.Parser;

namespace Reni.TokenClasses;

[BelongsTo(typeof(MainTokenFactory))]
sealed class ToNumberOfBase : Definable
{
    public const string TokenId = "to_number_of_base";
    public override string Id => TokenId;

    [DisableDump]
    internal override IEnumerable<IDeclarationProvider> MakeGeneric => this.GenericListFromDefinable(base.MakeGeneric);
}