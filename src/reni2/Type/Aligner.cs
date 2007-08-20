using Reni.Context;
using Reni.Parser;

namespace Reni.Type
{
    /// <summary>
    /// Performs alignement by extending the number of bytes a type uses.
    /// </summary>
    class Aligner : Child
    {
        int _alignBits;
        
        public Aligner(Base target, int alignBits) : base(target)
        {
            _alignBits = alignBits;
        }

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
        public override string DumpPrintText { get { return "#(#align" + _alignBits + "#)# " + Parent.DumpPrintText; } }

        /// <summary>
        /// Searches the definable token at type
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        public override SearchResult SearchDefineable(DefineableToken token)
        {
            SearchResult result = Parent.SearchDefineable(token);
            if(result != null)
                return result;
            return base.SearchDefineable(token);
        }

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
        /// <param name="useConverter">if set to <c>true</c> [use converter].</param>
        /// <returns>
        /// 	<c>true</c> if [is convertable to] [the specified dest]; otherwise, <c>false</c>.
        /// </returns>
        /// created 11.01.2007 22:09
        internal override bool IsConvertableToVirt(Base dest, bool useConverter)
        {
            return Parent.IsConvertableTo(dest, useConverter);
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
                delegate { return Code.Base.CreateArg(Size).CreateBitCast(Parent.Size); }
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