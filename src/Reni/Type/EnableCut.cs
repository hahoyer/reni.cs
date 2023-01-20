using hw.DebugFormatter;
using Reni.Feature;

namespace Reni.Type;

sealed class EnableCut
    : TagChild<TypeBase>
        , IForcedConversionProvider<NumberType>
{
    internal EnableCut(TypeBase parent)
        : base(parent)
        => Parent.IsCuttingPossible.Assert(Parent.Dump);

    IEnumerable<IConversion> IForcedConversionProvider<NumberType>.GetResult(NumberType destination)
        => Parent.CutEnabledConversion(destination);

    [DisableDump]
    protected override string TagTitle => "enable_cut";
}