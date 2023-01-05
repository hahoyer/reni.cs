using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Parser;
using Reni.Feature;
using Reni.Numeric;
using Reni.Parser;

namespace Reni.TokenClasses;

[BelongsTo(typeof(MainTokenFactory))]
[Variant(false)]
[Variant(true)]
sealed class EqualityOperation : Operation
{
    readonly bool IsEqual;

    public EqualityOperation(bool isEqual) => IsEqual = isEqual;

    public override string Id => TokenId(IsEqual);

    [DisableDump]
    internal override IEnumerable<IDeclarationProvider> MakeGeneric
        => this.GenericListFromDefinable(base.MakeGeneric);

    public static string TokenId(bool isEqual) => isEqual? "=" : "~=";
}