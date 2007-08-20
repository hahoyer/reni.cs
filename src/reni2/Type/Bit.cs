using HWClassLibrary.Debug;
using Reni.Context;
using Reni.Parser;
using Reni.Parser.TokenClass;

namespace Reni.Type
{
    /// <summary>
    /// Summary description for Bits.
    /// </summary>
    public class Bit : Primitive
    {
        internal Bit()
        {
        }

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
        public override string DumpPrintText { get { return "bit"; } }

        /// <summary>
        /// Gets a value indicating whether this instance has empty value.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has empty value; otherwise, <c>false</c>.
        /// </value>
        /// created 09.01.2007 03:21
        public override bool HasEmptyValue { get { return true; } }

        /// <summary>
        /// Searches the defineable from sequence.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        /// created 13.01.2007 19:35
        public override SearchResult SearchDefineableFromSequence(DefineableToken token, int count)
        {
            if (token.TokenClass.IsBitSequenceOperation)
                return new BitSequenceOperationSearchResult(CreateSequence(count), token.TokenClass);
            return null;
        }

        /// <summary>
        /// Searches the defineable prefix from sequence.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="count"></param>
        /// <returns></returns>
        /// created 02.02.2007 22:09
        internal override PrefixSearchResult PrefixSearchDefineableFromSequence(DefineableToken token, int count)
        {
            if (token.TokenClass.IsBitSequencePrefixOperation)
                return new BitSequenceOperationPrefixSearchResult(CreateSequence(count), token.TokenClass);
            return null;
        }

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
        /// Gets the type of the sequence element.
        /// </summary>
        /// <value>The type of the sequence element.</value>
        /// created 13.01.2007 19:46
        internal override int SequenceCount { get { return 1; } }

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
        /// Deep compare
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool TypedDeepEqual(Bit other)
        {
            return true;
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
            if (useConverter)
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
        internal override Code.Base CreateSequenceOperation(Defineable token, Result objResult, Size size, Result argResult)
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
        internal override Base SequenceOperationResultType(Defineable token, int objBitCount, int argBitCount)
        {
            return token.BitSequenceOperationResultType(objBitCount, argBitCount);
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

    internal sealed class BitSequenceOperationPrefixSearchResult : PrefixSearchResult
    {
        [DumpData(true)]
        private readonly Sequence _definingType;

        [DumpData(true)]
        private readonly Defineable _defineable;

        internal BitSequenceOperationPrefixSearchResult(Sequence definingType, Defineable defineable)
        {
            _definingType = definingType;
            _defineable = defineable;
        }

        internal override Result VisitApply(Category category, Result argResult)
        {
            Base elementType = argResult.Type.SequenceElementType;
            Result objResult = argResult.Type.VisitAsSequence(category, elementType).UseWithArg(argResult);
            Result result = new Result();
            if (category.HasSize || category.HasType || category.HasCode)
            {
                if (category.HasSize) result.Size = objResult.Size;
                if (category.HasType) result.Type = objResult.Type;
                if (category.HasCode) result.Code = elementType.CreateSequenceOperation(_defineable, objResult);
            };
            if (category.HasRefs) result.Refs = objResult.Refs;
            return result;
        }
    }

    internal sealed class BitSequenceOperationSearchResult : SearchResult
    {
        [DumpData(true)]
        private readonly Defineable _defineable;

        public BitSequenceOperationSearchResult(Sequence definingType, Defineable defineable)
            : base(definingType)
        {
            _defineable = defineable;
        }

        /// <summary>
        /// Creates the result for member function searched. Object is provided by use of "Arg" code element
        /// </summary>
        /// <param name="callContext">The call context.</param>
        /// <param name="category">The category.</param>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        /// <summary>
        /// Obtain result
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="category">The category.</param>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        public override Result VisitApply(Context.Base context, Category category, Syntax.Base args)
        {
            bool trace = ObjectId == 920 && context.ObjectId == 15 && category.HasType;
            StartMethodDumpWithBreak(trace, context, category, args);
            Type.Base elementType = DefiningType.SequenceElementType;
            Result objResult = DefiningType.VisitAsSequence(category, elementType);
            Result argResult = args.VisitAsSequence(context, category | Category.Type, elementType);
            if (trace) DumpMethodWithBreak("", context, category, args, "objResult", objResult, "argResult", argResult);
            Result result = new Result();
            if (category.HasSize || category.HasType || category.HasCode)
            {
                int objBitCount = DefiningType.UnrefSize.ToInt();
                int argBitCount = argResult.Type.UnrefSize.ToInt();
                Type.Base type = elementType.SequenceOperationResultType(_defineable, objBitCount, argBitCount).CreateAlign(context.RefAlignParam.AlignBits);
                if (category.HasSize) result.Size = type.Size;
                if (category.HasType) result.Type = type;
                if (category.HasCode) result.Code = elementType.CreateSequenceOperation(_defineable, objResult, type.Size, argResult);
            };
            if (category.HasRefs) result.Refs = objResult.Refs.Pair(argResult.Refs);
            return ReturnMethodDumpWithBreak(trace, result);
        }
    }
}