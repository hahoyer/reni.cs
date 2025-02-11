using hw.Parser;
using Reni.Basics;
using Reni.Feature;
using Reni.Parser;

namespace Reni.Numeric;

[BelongsTo(typeof(MainTokenFactory))]
sealed class Star : TransformationOperation
{
    public const string TokenId = "*";
    public override string Id => TokenId;

    protected override int Signature(int objSize, int argSize)
        => BitsConst.MultiplySize(objSize, argSize);

    [DisableDump]
    internal override IEnumerable<IDeclarationProvider> MakeGeneric
        => this.GenericListFromDefinable(base.MakeGeneric);
}