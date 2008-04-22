namespace Reni.Type
{
    internal sealed class EnableCut : TagChild
    {
        public EnableCut(TypeBase parent): base(parent) {}

        protected override string TagTitle { get { return "enable_cut"; } }

        internal override bool IsConvertableToVirt(TypeBase dest, ConversionFeature conversionFeature)
        {
            return base.IsConvertableToVirt(dest, conversionFeature.EnableCut);
        }
    }
}