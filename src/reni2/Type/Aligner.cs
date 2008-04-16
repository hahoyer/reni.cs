using Reni.Context;
using Reni.Parser;

namespace Reni.Type
{
    /// <summary>
    /// Performs alignement by extending the number of bytes a type uses.
    /// </summary>
    class Aligner : Child
    {
        readonly int _alignBits;
        
        public Aligner(Base target, int alignBits) : base(target)
        {
            _alignBits = alignBits;
        }

        /// <summary>
        /// Gets the align bits.
        /// </summary>
        /// <value>The align bits.</value>
        /// created 04.12.07 21:54 on HAHOYER-DELL by hh
        public int AlignBits { get { return _alignBits; } }

        public override Size Size
        {
            get
            {
                return Parent.Size.Align(AlignBits);
            }
         }

        /// <summary>
        /// Create a reference to a type
        /// </summary>
        /// <param name="refAlignParam">Alignment of the reference</param>
        /// <returns></returns>
        public override Ref CreateRef(RefAlignParam refAlignParam)
        {
            return Parent.CreateRef(refAlignParam.Align(AlignBits));
        }

        /// <summary>
        /// Gets the dump print text.
        /// </summary>
        /// <value>The dump print text.</value>
        /// created 08.01.2007 17:54
        internal override string DumpPrintText { get { return "#(#align" + _alignBits + "#)# " + Parent.DumpPrintText; } }

        /// <summary>
        /// Destructors the specified category.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        /// [created 02.06.2006 09:47]
        internal override Result DestructorHandler(Category category)
        {
            return Parent.DestructorHandler(category);
        }

        /// <summary>
        /// Moves the handler.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        /// [created 05.06.2006 16:47]
        internal override Result MoveHandler(Category category)
        {
            return Parent.MoveHandler(category);
        }

        internal override Result DumpPrintFromRef(Category category, RefAlignParam refAlignParam)
        {
            return Parent.DumpPrintFromRef(category, refAlignParam);
        }

        /// <summary>
        /// Applies the type operator.
        /// </summary>
        /// <param name="argResult">The arg result.</param>
        /// <returns></returns>
        /// created 10.01.2007 15:45
        public override Result ApplyTypeOperator(Result argResult)
        {
            return Parent.ApplyTypeOperator(argResult);
        }

        /// <summary>
        /// Determines whether [is convertable to] [the specified dest].
        /// </summary>
        /// <param name="dest">The dest.</param>
        /// <param name="conversionFeature">The conversion feature.</param>
        /// <returns>
        /// 	<c>true</c> if [is convertable to] [the specified dest]; otherwise, <c>false</c>.
        /// </returns>
        /// created 11.01.2007 22:09
        internal override bool IsConvertableToVirt(Base dest, ConversionFeature conversionFeature)
        {
            return Parent.IsConvertableTo(dest, conversionFeature);
        }
        /// <summary>
        /// Converts to.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="dest">The dest.</param>
        /// <returns></returns>
        /// created 11.01.2007 22:12
        internal override Result ConvertToVirt(Category category, Base dest)
        {
            return Parent
                .ConvertTo(category, dest)
                .UseWithArg(CreateUnalignedArgResult(category));
        }

        private Result CreateUnalignedArgResult(Category category)
        {
            return Parent.CreateResult
                (
                category,
                () => Code.Base.CreateArg(Size).CreateBitCast(Parent.Size)
                );
        }

        /// <summary>
        /// Gets the type of the sequence element.
        /// </summary>
        /// <value>The type of the sequence element.</value>
        /// created 13.01.2007 19:46
        internal override int SequenceCount { get { return Parent.SequenceCount; } }

        /// <summary>
        /// Determines whether [has converter to] [the specified dest].
        /// </summary>
        /// <param name="dest">The dest.</param>
        /// <returns>
        /// 	<c>true</c> if [has converter to] [the specified dest]; otherwise, <c>false</c>.
        /// </returns>
        internal override bool HasConverterTo(Base dest)
        {
            return Parent.HasConverterTo(dest);
        }
    }
}