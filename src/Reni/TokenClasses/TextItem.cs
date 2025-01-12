using hw.Parser;
using Reni.Feature;
using Reni.Parser;

namespace Reni.TokenClasses;

[BelongsTo(typeof(MainTokenFactory))]
sealed class TextItem : Definable
{
    public const string TokenId = "text_item";

    [DisableDump]
    internal override IEnumerable<IDeclarationProvider> MakeGeneric
        => this.GenericListFromDefinable(base.MakeGeneric);

    public override string Id => TokenId;
}