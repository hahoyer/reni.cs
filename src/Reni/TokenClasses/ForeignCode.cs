using hw.Parser;
using Reni.Feature;
using Reni.Parser;

namespace Reni.TokenClasses;

[BelongsTo(typeof(MainTokenFactory))]
sealed class ForeignCode : Definable
{
    public const string TokenId = "|\\|";
    public override string Id => TokenId;

    [DisableDump]
    internal override IEnumerable<IDeclarationProvider> MakeGeneric
        => this.GenericListFromDefinable(base.MakeGeneric);
}
