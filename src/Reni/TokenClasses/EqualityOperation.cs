using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Parser;
using Reni.Feature;
using Reni.Numeric;
using Reni.Parser;

namespace Reni.TokenClasses;

[BelongsTo(typeof(MainTokenFactory))]
[Variant(false, false)]
[Variant(true, false)]
[Variant(false, true)]
[Variant(true, true)]
sealed class EqualityOperation : Operation
{
    readonly bool Identity;
    readonly bool IsEqual;

    public EqualityOperation(bool isEqual, bool identity)
    {
        Identity = identity;
        IsEqual = isEqual;
    }

    public override string Id => TokenId(IsEqual, Identity);

    [DisableDump]
    internal override IEnumerable<IDeclarationProvider> MakeGeneric
        => this.GenericListFromDefinable(base.MakeGeneric);

    public static string TokenId(bool isEqual, bool identity)
        => isEqual? identity? "==" : "=" :
            identity? "~==" : "~=";
}