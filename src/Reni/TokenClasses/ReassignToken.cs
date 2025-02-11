using hw.Parser;
using Reni.Feature;
using Reni.Parser;

namespace Reni.TokenClasses;

[BelongsTo(typeof(MainTokenFactory))]
sealed class ReassignToken : Definable
{
    public const string TokenId = ":=";
    public override string Id => TokenId;

    [DisableDump]
    internal override IEnumerable<IDeclarationProvider> MakeGeneric
        => this.GenericListFromDefinable(base.MakeGeneric);
}

[BelongsTo(typeof(MainTokenFactory))]
sealed class ForceMutabilityToken : Definable
{
    public const string TokenId = "force_mutability";
    public override string Id => TokenId;

    [DisableDump]
    internal override IEnumerable<IDeclarationProvider> MakeGeneric
        => this.GenericListFromDefinable(base.MakeGeneric);
}

[BelongsTo(typeof(MainTokenFactory))]
sealed class Mutable : Definable
{
    public const string TokenId = "mutable";
    public override string Id => TokenId;

    [DisableDump]
    internal override IEnumerable<IDeclarationProvider> MakeGeneric
        => this.GenericListFromDefinable(base.MakeGeneric);
}

[BelongsTo(typeof(MainTokenFactory))]
sealed class EnableReinterpretation : Definable
{
    public const string TokenId = "enable_reinterpretation";
    public override string Id => TokenId;

    [DisableDump]
    internal override IEnumerable<IDeclarationProvider> MakeGeneric
        => this.GenericListFromDefinable(base.MakeGeneric);
}