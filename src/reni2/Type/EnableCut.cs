namespace Reni.Type
{
    internal sealed class EnableCut : TagChild
    {
        public EnableCut(TypeBase parent)
            : base(parent) {}

        protected override string TagTitle { get { return "enable_cut"; } }

        /// <summary>
        /// Determines whether [is convertable to virt] [the specified dest].
        /// </summary>
        /// <param name="dest">The dest.</param>
        /// <param name="conversionFeature">The conversion feature.</param>
        /// <returns>
        /// 	<c>true</c> if [is convertable to virt] [the specified dest]; otherwise, <c>false</c>.
        /// </returns>
        /// created 30.01.2007 22:42
        internal override bool IsConvertableToVirt(TypeBase dest, ConversionFeature conversionFeature)
        {
            return base.IsConvertableToVirt(dest, conversionFeature.EnableCut);
        }
    }
}