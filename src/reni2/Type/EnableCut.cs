using System;

namespace Reni.Type
{
    [Serializable]
    internal sealed class EnableCut : TagChild
    {
        internal EnableCut(TypeBase parent): base(parent) {}

        protected override string TagTitle { get { return "enable_cut"; } }

        internal override bool IsConvertableTo_Implementation(TypeBase dest, ConversionFeature conversionFeature)
        {
            return base.IsConvertableTo_Implementation(dest, conversionFeature.EnableCut);
        }

    }
}