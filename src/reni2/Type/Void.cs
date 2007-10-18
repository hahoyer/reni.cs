namespace Reni.Type
{
    sealed internal class Void : Primitive
    {
        /// <summary>
        /// The size of type
        /// </summary>
        public override Size Size { get { return Size.Zero; } }
        /// <summary>
        /// Gets a value indicating whether this instance is void.
        /// </summary>
        /// <value><c>true</c> if this instance is void; otherwise, <c>false</c>.</value>
        /// [created 05.06.2006 16:27]
        public override bool IsVoid { get { return true; } }

        /// <summary>
        /// Creates the pair.
        /// </summary>
        /// <param name="second">The second.</param>
        /// <returns></returns>
        /// created 19.11.2006 22:56
        public override Base CreatePair(Base second)
        {
            return second;
        }

        protected override Base CreateReversePair(Base first)
        {
            return first;
        }

        /// <summary>
        /// Gets the dump print text.
        /// </summary>
        /// <value>The dump print text.</value>
        /// created 08.01.2007 17:54
        internal override string DumpPrintText { get { return "void"; } }

        /// <summary>
        /// Dumps the print code.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        /// created 08.01.2007 17:29
        internal override Result DumpPrint(Category category)
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
        internal override bool IsConvertableToVirt(Base dest, ConversionFeature conversionFeature)
        {
            return false;
        }

        /// <summary>
        /// Creates the result.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        /// created 08.01.2007 18:11
        /// created 16.05.2007 22:38 on HAHOYER-DELL by hh
        internal new static Result CreateResult(Category category)
        {
            return CreateVoid.CreateResult(category);
        }

        /// <summary>
        /// Creates the result.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="getCode">The get code.</param>
        /// <returns></returns>
        /// created 08.01.2007 14:38
        /// created 16.05.2007 23:13 on HAHOYER-DELL by hh
        internal new static Result CreateResult(Category category, Result.GetCode getCode)
        {
            return CreateVoid.CreateResult(category,getCode);
        }

        /// <summary>
        /// Creates the result.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="getCode">The get code.</param>
        /// <param name="getRefs">The get refs.</param>
        /// <returns></returns>
        /// created 08.01.2007 14:38
        /// created 16.05.2007 23:13 on HAHOYER-DELL by hh
        internal new static Result CreateResult(Category category, Result.GetCode getCode, Result.GetRefs getRefs)
        {
            return CreateVoid.CreateResult(category, getCode, getRefs);
        }
    }
}
