namespace Reni.Type
{
    /// <summary>
    /// 
    /// </summary>
    internal class Pending : TypeBase
    {
        /// <summary>
        /// The size of type
        /// </summary>
        public override Size Size { get { return Size.Pending; } }

        /// <summary>
        /// Gets the dump print text.
        /// </summary>
        /// <value>The dump print text.</value>
        /// created 08.01.2007 17:54
        internal override string DumpPrintText { get { return "#(# Prendig type #)#"; } }

        /// <summary>
        /// Gets a value indicating whether this instance is pending.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is pending; otherwise, <c>false</c>.
        /// </value>
        /// created 09.02.2007 00:26
        internal override bool IsPending { get { return true; } }

        /// <summary>
        /// Converts to.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="dest">The dest.</param>
        /// <returns></returns>
        /// created 11.01.2007 22:12
        internal override Result ConvertToVirt(Category category, TypeBase dest)
        {
            return dest.CreateResult
                (
                category,
                () => Code.Base.Pending,
                () => Refs.Pending
                );
        }

        /// <summary>
        /// Visits as sequence.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="elementType">Type of the element.</param>
        /// <returns></returns>
        /// created 13.01.2007 22:20
        internal override Result VisitAsSequence(Category category, TypeBase elementType)
        {
            return CreateResult(category);
        }

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
            return true;
        }
    }
}