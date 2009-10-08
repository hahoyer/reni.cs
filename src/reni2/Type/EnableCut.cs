using System;

namespace Reni.Type
{
    [Serializable]
    internal sealed class EnableCut : TagChild
    {
        internal EnableCut(TypeBase parent): base(parent) {}

        protected override string TagTitle { get { return "enable_cut"; } }

        internal override bool IsValidRefTarget() { return Parent.IsValidRefTarget(); }

        internal override bool IsConvertableToImplementation(TypeBase dest, ConversionFeature conversionFeature)
        {
            return base.IsConvertableToImplementation(dest, conversionFeature.EnableCut);
        }

    }
}