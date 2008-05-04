using Reni.Code;

namespace Reni.Type
{
    sealed internal class Pending : TypeBase
    {
        internal override Size Size { get { return Size.Pending; } }
        internal override string DumpPrintText { get { return "#(# Prendig type #)#"; } }
        internal override bool IsPending { get { return true; } }

        internal override Result ConvertToVirt(Category category, TypeBase dest)
        {
            return dest.CreateResult
                (
                category,
                () => CodeBase.Pending,
                () => Refs.Pending
                );
        }

        internal override bool IsConvertableToVirt(TypeBase dest, ConversionFeature conversionFeature)
        {
            return true;
        }
    }
}