using HWClassLibrary.Debug;
using Reni.Feature;
using Reni.Parser;
using Reni.Parser.TokenClass;

namespace Reni.Type
{
    /// <summary>
    /// Summary description for Bits.
    /// </summary>
    internal sealed class Bit : Primitive
    {
        /// <summary>
        /// asis
        /// </summary>
        [DumpData(false)]
        public override Size Size { get { return Size.Create(1); } }

        /// <summary>
        /// Gets the dump print text.
        /// </summary>
        /// <value>The dump print text.</value>
        /// created 08.01.2007 17:54
        internal override string DumpPrintText { get { return "bit"; } }

        /// <summary>
        /// Gets a value indicating whether this instance has empty value.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has empty value; otherwise, <c>false</c>.
        /// </value>
        /// created 09.01.2007 03:21
        public override bool HasEmptyValue { get { return true; } }

        /// <summary>
        /// Gets the type of the sequence element.
        /// </summary>
        /// <value>The type of the sequence element.</value>
        /// created 13.01.2007 19:46
        internal override int SequenceCount { get { return 1; } }

        /// <summary>
        /// Dumps the print code from array.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        /// created 08.01.2007 17:29
        internal override Result SequenceDumpPrint(Category category, int count)
        {
            return CreateSequence(count).CreateArgResult(category).DumpPrintBitSequence();
        }

        /// <summary>
        /// Dumps the print code from array.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns></returns>               
        /// created 08.01.2007 17:29
        internal override Result DumpPrint(Category category)
        {
            return CreateArgResult(category).DumpPrintBitSequence();
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
        internal override bool IsConvertableToVirt(TypeBase dest, ConversionFeature conversionFeature)
        {
            if(conversionFeature.IsUseConverter)
                return dest.HasConverterFromBit;

            return false;
        }

        /// <summary>
        /// Creates the operation.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="argResult">The arg result.</param>
        /// <param name="objResult">The obj result.</param>
        /// <param name="size">The size.</param>
        /// <returns></returns>
        /// created 13.01.2007 21:18
        internal override Code.Base CreateSequenceOperation(Defineable token, Result objResult, Size size,
            Result argResult)
        {
            return objResult.Code.CreateBitSequenceOperation(token, size, argResult.Code);
        }

        /// <summary>
        /// Creates the operation.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        /// created 02.02.2007 23:28
        internal override Code.Base CreateSequenceOperation(Defineable token, Result result)
        {
            return result.Code.CreateBitSequenceOperation(token);
        }

        /// <summary>
        /// Operations the type of the result.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="objBitCount">The obj bit count.</param>
        /// <param name="argBitCount">The arg bit count.</param>
        /// <returns></returns>
        /// created 13.01.2007 21:43
        internal override TypeBase SequenceOperationResultType(Defineable token, int objBitCount, int argBitCount)
        {
            return token.BitSequenceOperationResultType(objBitCount, argBitCount);
        }

        internal override SearchResult<IFeature> SearchFromSequence(Defineable defineable)
        {
            return defineable.SearchFromSequenceOfBit();
        }
        internal override SearchResult<IPrefixFeature> SearchPrefixFromSequence(Defineable defineable)
        {
            return defineable.SearchPrefixFromSequenceOfBit();
        }

        /// <summary>
        /// Default dump behaviour
        /// </summary>
        /// <returns></returns>
        /// created 07.05.2007 21:15 on HAHOYER-DELL by hh
        public override string Dump()
        {
            return GetType().FullName;
        }
    }

}