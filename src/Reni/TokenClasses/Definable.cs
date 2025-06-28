using hw.Parser;
using Reni.Feature;
using Reni.Parser;
using Reni.SyntaxFactory;

namespace Reni.TokenClasses;

abstract class Definable
    : TokenClass, IDeclarationTag, IValueToken
{
    [DisableDump]
    protected string DataFunctionName => Id.Symbolize();

    IValueProvider IValueToken.Provider => Factory.Definable;

    [DisableDump]
    internal virtual IEnumerable<IDeclarationProvider> MakeGeneric
        => this.GenericListFromDefinable();
}

[BelongsTo(typeof(MainTokenFactory))]
sealed class ConcatArrays : Definable
{
    public const string TokenId = "<<";

    [DisableDump]
    internal override IEnumerable<IDeclarationProvider> MakeGeneric
        => this.GenericListFromDefinable(base.MakeGeneric);

    public override string Id => TokenId;
}

[BelongsTo(typeof(MainTokenFactory))]
sealed class MutableConcatArrays : Definable
{
    public const string TokenId = "<<:=";

    [DisableDump]
    internal override IEnumerable<IDeclarationProvider> MakeGeneric
        => this.GenericListFromDefinable(base.MakeGeneric);

    public override string Id => TokenId;
}

[BelongsTo(typeof(MainTokenFactory))]
sealed class Count : Definable
{
    public const string TokenId = "count";

    [DisableDump]
    internal override IEnumerable<IDeclarationProvider> MakeGeneric
        => this.GenericListFromDefinable(base.MakeGeneric);

    public override string Id => TokenId;
}

[BelongsTo(typeof(MainTokenFactory))]
sealed class StableReference : Definable
{
    public const string TokenId = "stable_reference";

    [DisableDump]
    internal override IEnumerable<IDeclarationProvider> MakeGeneric
        => this.GenericListFromDefinable(base.MakeGeneric);

    public override string Id => TokenId;
}

[BelongsTo(typeof(MainTokenFactory))]
sealed class ArrayReference : Definable
{
    public const string TokenId = "array_reference";

    [DisableDump]
    internal override IEnumerable<IDeclarationProvider> MakeGeneric
        => this.GenericListFromDefinable(base.MakeGeneric);

    public override string Id => TokenId;
}

[BelongsTo(typeof(MainTokenFactory))]
sealed class SystemText : Definable
{
    public const string TokenId = "Text";

    [DisableDump]
    internal override IEnumerable<IDeclarationProvider> MakeGeneric
        => this.GenericListFromDefinable(base.MakeGeneric);

    public override string Id => TokenId;
}
