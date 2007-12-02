using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper.TreeViewSupport;
using Reni.Context;
using Reni.Parser;
using Reni.Parser.TokenClass;

namespace Reni.Type
{
    /// <summary>
    /// Summary description for Base.
    /// </summary>
    [AdditionalNodeInfo("DebuggerDumpString")]
    internal abstract class Base : ReniObject
    {
        private readonly HWClassLibrary.Helper.DictionaryEx<int, Aligner> _aligner = new HWClassLibrary.Helper.DictionaryEx<int, Aligner>();
        private readonly HWClassLibrary.Helper.DictionaryEx<int, Array> _array = new HWClassLibrary.Helper.DictionaryEx<int, Array>();
        private readonly HWClassLibrary.Helper.DictionaryEx<int, Sequence> _chain = new HWClassLibrary.Helper.DictionaryEx<int, Sequence>();
        private readonly HWClassLibrary.Helper.DictionaryEx<Base, Pair> _pair = new HWClassLibrary.Helper.DictionaryEx<Base, Pair>();
        private readonly HWClassLibrary.Helper.DictionaryEx<RefAlignParam, Ref> _ref = new HWClassLibrary.Helper.DictionaryEx<RefAlignParam, Ref>();
        private EnableCut _enableCutCache;
        private TypeType _typeTypeCache;
        private static readonly Bit _bit = new Bit();
        private static readonly Void _void = new Void();

        static private Pending _pending = null;

        public Base(int objectId)
            : base(objectId)
        {
        }

        public Base()
        {
        }

        /// <summary>
        /// The size of type
        /// </summary>
        [Node]
        public abstract Size Size { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is ref.
        /// </summary>
        /// <value><c>true</c> if this instance is ref; otherwise, <c>false</c>.</value>
        /// [created 01.06.2006 22:51]
        [DumpData(false)]
        virtual public bool IsRef { get { return false; } }

        /// <summary>
        /// Gets a value indicating whether this instance is void.
        /// </summary>
        /// <value><c>true</c> if this instance is void; otherwise, <c>false</c>.</value>
        /// [created 05.06.2006 16:27]
        [DumpData(false)]
        virtual public bool IsVoid { get { return false; } }

        /// <summary>
        /// Gets the size of the unref.
        /// </summary>
        /// <value>The size of the unref.</value>
        /// [created 06.06.2006 00:08]
        [DumpData(false)]
        virtual public Size UnrefSize { get { return Size; } }

        /// <summary>
        /// Creates the void.type instance
        /// </summary>
        /// <returns></returns>
        /// created 08.01.2007 01:43
        [DumpData(false)]
        public static Base CreateVoid { get { return _void; } }

        /// <summary>
        /// Creates the bit.type instance
        /// </summary>
        [DumpData(false)]
        static public Base CreateBit { get { return _bit; } }

        /// <summary>
        /// Create aligner type
        /// </summary>
        /// <param name="alignBits"></param>
        /// <returns></returns>
        public Base CreateAlign(int alignBits)
        {
            if (Size.Align(alignBits) == Size)
                return this;
            return _aligner.Find(alignBits, delegate { return new Aligner(this, alignBits); });
        }

        /// <summary>
        /// Creates array type
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public Array CreateArray(int count)
        {
            return _array.Find(count, delegate { return new Array(this, count); });
        }

        internal EnableCut CreateEnableCut()
        {
            if (_enableCutCache == null)
                _enableCutCache = new EnableCut(this);
            return _enableCutCache;
        }
        /// <summary>
        /// Creates the number.
        /// </summary>
        /// <param name="bitCount">The bit count.</param>
        /// <returns></returns>
        /// created 13.01.2007 23:45
        public static Base CreateNumber(int bitCount)
        {
            return CreateBit.CreateSequence(bitCount);
        }
        /// <summary>
        /// Creates the pair.
        /// </summary>
        /// <param name="second">The second.</param>
        /// <returns></returns>
        /// created 19.11.2006 22:56
        public virtual Base CreatePair(Base second)
        {
            return second.CreateReversePair(this);
        }

        protected virtual Base CreateReversePair(Base first)
        {
            return first._pair.Find(this, delegate { return new Pair(first, this); });
        }

        /// <summary>
        /// Create a reference to a type
        /// </summary>
        /// <param name="refAlignParam">Alignment  and size of the reference</param>
        /// <returns></returns>
        public virtual Ref CreateRef(RefAlignParam refAlignParam)
        {
            return _ref.Find(refAlignParam, delegate { return new Ref(this, refAlignParam); });
        }
        /// <summary>
        /// Create chain type
        /// </summary>
        /// <param name="elementCount">The elementCount.</param>
        /// <returns></returns>
        public Sequence CreateSequence(int elementCount)
        {
            return _chain.Find(elementCount, delegate { return new Sequence(this, elementCount); });
        }

        private Base TypeType
        {
            get
            {
                if (_typeTypeCache == null)
                    _typeTypeCache = new TypeType(this);
                return _typeTypeCache;
            }
        }

        /// <summary>
        /// Gets the dump print text.
        /// </summary>
        /// <value>The dump print text.</value>
        /// created 08.01.2007 17:54
        [DumpData(false)]
        internal virtual string DumpPrintText
        {
            get
            {
                NotImplementedMethod();
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has empty value.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has empty value; otherwise, <c>false</c>.
        /// </value>
        /// created 09.01.2007 03:21
        [DumpData(false)]
        virtual public bool HasEmptyValue
        {
            get
            {
                NotImplementedMethod();
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Searches the defineable prefix.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        /// created 02.02.2007 21:51
        internal virtual PrefixSearchResult PrefixSearchDefineable(DefineableToken token)
        {
            return null;
        }

        /// <summary>
        /// Searches the defineable prefix from sequence.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        /// created 02.02.2007 22:09
        internal virtual PrefixSearchResult PrefixSearchDefineableFromSequence(DefineableToken token, int count)
        {
            return null;
        }
        /// <summary>
        /// Destructors the specified category.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        /// [created 02.06.2006 09:47]
        internal virtual Result DestructorHandler(Category category)
        {
            NotImplementedMethod(category);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Arrays the destructor.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        /// [created 04.06.2006 00:51]
        internal virtual Result ArrayDestructorHandler(Category category, int count)
        {
            NotImplementedMethod(category, count);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Moves the handler.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        /// [created 05.06.2006 16:47]
        internal virtual Result MoveHandler(Category category)
        {
            NotImplementedMethod(category);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Arrays the move handler.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        /// [created 05.06.2006 16:54]
        internal virtual Result ArrayMoveHandler(Category category, int count)
        {
            NotImplementedMethod(category, count);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates the arg.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        /// [created 04.06.2006 01:04]
        internal Result CreateArgResult(Category category)
        {
            return CreateResult(category, delegate { return CreateArgCode(); });
        }

        /// <summary>
        /// Creates the arg code.
        /// </summary>
        /// <returns></returns>
        /// created 30.01.2007 23:40
        internal Code.Base CreateArgCode()
        {
            return Code.Base.CreateArg(Size);
        }

        /// <summary>
        /// Creates the result.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        /// created 08.01.2007 18:11
        internal Result CreateResult(Category category)
        {
            return CreateResult(category, delegate { return Code.Base.CreateBitArray(Size, BitsConst.Convert(0).Resize(Size)); });
        }

        /// <summary>
        /// Creates the result.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="codeAndRefs">The code and refs.</param>
        /// <returns></returns>
        /// created 08.01.2007 18:11
        internal Result CreateResult(Category category, Result codeAndRefs)
        {
            Result result = new Result();
            if (category.HasSize) result.Size = Size;
            if (category.HasType) result.Type = this;
            if (category.HasCode) result.Code = codeAndRefs.Code;
            if (category.HasRefs) result.Refs = codeAndRefs.Refs;
            return result;
        }

        /// <summary>
        /// Creates the result.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="getCode">The get code.</param>
        /// <returns></returns>
        /// created 08.01.2007 14:38
        internal Result CreateResult(Category category, Result.GetCode getCode)
        {
            return CreateResult(category, getCode, delegate { return Refs.None(); });
        }

        /// <summary>
        /// Creates the result.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        /// created 08.01.2007 14:38
        public Result CreateContextRefResult<C>(Category category, C context) where C : Context.Base
        {
            return CreateResult(
                category, 
                delegate { return Code.Base.CreateContextRef(context); }, 
                delegate { return Refs.Context(context); });
        }

        /// <summary>
        /// Creates the result.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="getCode">The get code.</param>
        /// <param name="getRefs">The get refs.</param>
        /// <returns></returns>
        /// created 08.01.2007 14:38
        internal Result CreateResult(Category category, Result.GetCode getCode, Result.GetRefs getRefs)
        {
            Result result = new Result();
            if (category.HasSize) result.Size = Size;
            if (category.HasType) result.Type = this;
            if (category.HasCode) result.Code = getCode();
            if (category.HasRefs) result.Refs = getRefs();
            return result;
        }

        /// <summary>
        /// Applies the function.
        /// </summary>
        /// <param name="callContext">The context.</param>
        /// <param name="category">The category.</param>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        /// created 29.10.2006 18:24
        internal virtual Result ApplyFunction(Category category, Context.Base callContext, Syntax.Base args)
        {
            NotImplementedMethod(callContext, category, args);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Applies a function.with argumenst evaluated
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="argsResult">The args result.</param>
        /// <returns></returns>
        /// Created 18.11.07 15:32 by hh on HAHOYER-DELL
        virtual internal Result ApplyFunction(Category category, Result argsResult)
        {
            NotImplementedMethod(category, argsResult);
            return null;
        }

        virtual internal Result PostProcess(Ref visitedType, Result result)
        {
            if(this == visitedType.Target)
                return result.UseWithArg(visitedType.CreateDereferencedArgResult(result.Complete));
            NotImplementedMethod(visitedType, result);
            return null;
        }

        /// <summary>
        /// Empties the handler.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        /// created 30.12.2006 16:40
        public static Result EmptyHandler(Category category)
        {
            return CreateVoidResult(category - Category.Type - Category.Size);
        }

        /// <summary>
        /// Creates the void result.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        /// created 10.01.2007 02:58
        public static Result CreateVoidResult(Category category)
        {
            return CreateVoid.CreateResult(category);
        }

        /// <summary>
        /// Checks if type is a reference and dereferences instance.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        /// created 05.01.2007 01:10
        virtual public Result Dereference(Result result)
        {
            return result;
        }

        /// <summary>
        /// Types the operator.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        /// created 07.01.2007 21:14
        public Result TypeOperator(Category category)
        {
            Result result = CreateVoidResult(category).Clone();
            if (category.HasType) result.Type = TypeType;
            return result;
        }

        /// <summary>
        /// Dumps the print code.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        /// created 08.01.2007 17:29
        internal virtual Result DumpPrint(Category category)
        {
            NotImplementedMethod(category);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates the result for DumpPrint-call. Object is provided as reference by use of "Arg" code element
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="refAlignParam">The ref align param.</param>
        /// <returns></returns>
        /// created 15.05.2007 23:42 on HAHOYER-DELL by hh
        virtual internal Result DumpPrintFromRef(Category category, RefAlignParam refAlignParam)
        {
            Result argResult = CreateRef(refAlignParam).Conversion(category, this);
            return DumpPrint(category).UseWithArg(argResult);
        }
        /// <summary>
        /// Dumps the print code from array.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        /// created 08.01.2007 17:29
        virtual public Result ArrayDumpPrint(Category category, int count)
        {
            NotImplementedMethod(category, count);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Dumps the print code from array.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        /// created 08.01.2007 17:29
        internal virtual Result SequenceDumpPrint(Category category, int count)
        {
            NotImplementedMethod(category, count);
            throw new NotImplementedException();
        }

        /// <summary>
        /// type where the dump print is implemented (array variant).
        /// </summary>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        /// created 08.01.2007 17:33
        virtual public Base DumpPrintArrayType(int count)
        {
            NotImplementedMethod(count);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Applies the type operator.
        /// </summary>
        /// <param name="argResult">The arg result.</param>
        /// <returns></returns>
        /// created 10.01.2007 15:45
        virtual public Result ApplyTypeOperator(Result argResult)
        {
            return argResult.Type.Conversion(argResult.Complete, this).UseWithArg(argResult);
        }

        // Conversion
 
        /// <summary>
        /// Commons the type.
        /// </summary>
        /// <param name="dest">The dest.</param>
        /// <returns></returns>
        /// created 09.01.2007 01:20
        public Base CommonType(Base dest)
        {
            if(IsConvertableTo(dest,ConversionFeature.Instance))
                return dest;
            if (dest.IsConvertableTo(this, ConversionFeature.Instance))
                return this;
            NotImplementedMethod(dest);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Main conversion function. Creates the results for conversion of one type into another.
        /// </summary>
        /// <param name="category">Categories to obtain.</param>
        /// <param name="dest">The destination type.</param>
        /// <returns></returns>
        /// created 11.01.2007 21:26
        public Result Conversion(Category category, Base dest)
        {
            if(category.HasCode || category.HasRefs)
            {
                if (IsConvertableTo(dest, ConversionFeature.Instance))
                    return ConvertTo(category, dest);
                NotImplementedMethod(category, dest);
                throw new NotImplementedException();
            }
            return dest.CreateResult(category);
        }

        /// <summary>
        /// Internal conversion function. Used only inside of conversion strategy. From outside call <see cref="Base.Conversion"/>.
        /// </summary>
        /// <param name="category">Categories to obtain.</param>
        /// <param name="dest">The destination type.</param>
        /// <returns></returns>
        /// created 11.01.2007 22:12
        internal Result ConvertTo(Category category, Base dest)
        {
            if (this == dest)
                return ConvertToItself(category);
            return ConvertToVirt(category, dest);
        }

        /// <summary>
        /// Internal conversion function. Used only inside of conversion strategy. From outside call <see cref="Base.Conversion"/>.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        /// created 30.01.2007 22:57
        internal virtual Result ConvertToItself(Category category)
        {
            return CreateArgResult(category);
        }

        /// <summary>
        /// Converts to.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="dest">The dest.</param>
        /// <returns></returns>
        /// created 11.01.2007 22:12
        internal virtual Result ConvertToVirt(Category category, Base dest)
        {
            NotImplementedMethod(category, dest);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a value indicating whether this instance has converter from bit.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has converter from bit; otherwise, <c>false</c>.
        /// </value>
        /// created 11.01.2007 22:43
        [DumpData(false)]
        internal virtual bool HasConverterFromBit
        {
            get
            {
                NotImplementedMethod();
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets the type of the sequence element.
        /// </summary>
        /// <value>The type of the sequence element.</value>
        /// created 13.01.2007 19:46
        [DumpData(false)]
        internal virtual Base SequenceElementType
        {
            get
            {
                NotImplementedMethod();
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets the type of the sequence element.
        /// </summary>
        /// <value>The type of the sequence element.</value>
        /// created 13.01.2007 19:46
        [DumpData(false)]
        internal virtual int SequenceCount
        {
            get
            {
                NotImplementedMethod(); 
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets the type in case of pending visits
        /// </summary>
        /// <value>The pending.</value>
        /// created 24.01.2007 22:23
        internal static Base Pending
        {
            get
            {
                if(_pending == null)
                    _pending = new Pending();

                return _pending;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is pending.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is pending; otherwise, <c>false</c>.
        /// </value>
        /// created 09.02.2007 00:26
        [DumpData(false)]
        internal virtual bool IsPending { get { return false; } }

        [DumpData(false)]
        virtual internal protected string DumpPrintTextPair
        {
            get
            {
                return "\n"+ DumpPrintText;
            }
        }

        /// <summary>
        /// Visits as sequence.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="elementType">Type of the element.</param>
        /// <returns></returns>
        /// created 13.01.2007 22:20
        internal virtual Result VisitAsSequence(Category category, Base elementType)
        {
            int count = SequenceCount;
            Base resultType = elementType.CreateSequence(count);
            return Conversion(category,resultType);
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
        internal bool IsConvertableTo(Base dest, ConversionFeature conversionFeature)
        {
            if (this == dest)
                return IsConvertableToItself(conversionFeature);
            if(conversionFeature.IsUseConverter && HasConverterTo(dest))
                return true;
            return IsConvertableToVirt(dest, conversionFeature);
        }

        /// <summary>
        /// Determines whether [has converter to] [the specified dest].
        /// </summary>
        /// <param name="dest">The dest.</param>
        /// <returns>
        /// 	<c>true</c> if [has converter to] [the specified dest]; otherwise, <c>false</c>.
        /// </returns>
        internal virtual bool HasConverterTo(Base dest)
        {
            return false;
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
        internal virtual bool IsConvertableToVirt(Base dest, ConversionFeature conversionFeature)
        {
            NotImplementedMethod(dest, conversionFeature);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Determines whether [is convertable to itself] [the specified use converter].
        /// </summary>
        /// <param name="conversionFeature">The conversion feature.</param>
        /// <returns>
        /// 	<c>true</c> if [is convertable to itself] [the specified use converter]; otherwise, <c>false</c>.
        /// </returns>
        /// created 30.01.2007 23:02
        internal virtual bool IsConvertableToItself(ConversionFeature conversionFeature)
        {
            return true;
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
        internal virtual Code.Base CreateSequenceOperation(Defineable token, Result objResult, Size size, Result argResult)
        {
            NotImplementedMethod(token, objResult, size, argResult.Code);
            return null;
        }

        /// <summary>
        /// Creates the operation.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        /// created 02.02.2007 23:28
        internal virtual Code.Base CreateSequenceOperation(Defineable token, Result result)
        {
            NotImplementedMethod(token, result);
            return null;
        }
        /// <summary>
        /// Operations the type of the result.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="objBitCount">The obj bit count.</param>
        /// <param name="argBitCount">The arg bit count.</param>
        /// <returns></returns>
        /// created 13.01.2007 21:43
        internal virtual Base SequenceOperationResultType(Defineable token, int objBitCount, int argBitCount)
        {
            NotImplementedMethod(token,objBitCount,argBitCount);
            return null;
        }

        /// <summary>
        /// Thens the else with pending.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="condRefs">The cond refs.</param>
        /// <param name="elseOrThenRefs">The else or then refs.</param>
        /// <returns></returns>
        internal Result ThenElseWithPending(Category category, Refs condRefs, Refs elseOrThenRefs)
        {
            Tracer.Assert(!category.HasCode);

            return CreateResult
                (
                category,
                delegate { return null; },
                delegate { return condRefs.Pair(elseOrThenRefs); }
                );
        }

        /// <summary>
        /// Creates the ref code for context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        /// created 01.07.07 19:16 on HAHOYER-DELL by h
        internal virtual Code.Base CreateRefCodeForContext(Context.Base context)
        {
            NotImplementedMethod(context);
            return null;
        }

        /// <summary>
        /// If type is property, execute it.
        /// </summary>
        /// <param name="rawResult">The result.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        /// created 30.07.2007 21:28 on HAHOYER-DELL by hh
        virtual internal Result UnProperty(Result rawResult, Context.Base context)
        {
            return rawResult;
        }

        /// <summary>
        /// Searches the specified defineable.
        /// </summary>
        /// <param name="defineableToken">The defineable.</param>
        /// <returns></returns>
        /// Created 04.11.07 17:51 by hh on HAHOYER-DELL
        virtual internal SearchResult Search(DefineableToken defineableToken)
        {
            NotImplementedMethod(defineableToken);
            return null;
        }

        virtual internal SearchResultFromSequence SearchFromSequence(Defineable defineable)
        {
            NotImplementedMethod(defineable);
            return null;
        }

        virtual internal SearchResultFromRef SearchFromRef(DefineableToken defineableToken, Ref definingType)
        {
            return defineableToken.TokenClass.SearchFromRef(defineableToken, definingType);
        }
    }

    internal abstract class SearchResultFromSequence: ReniObject
    {
        internal abstract SearchResult ToSearchResult(Sequence sequence);
    }

    internal class ConversionFeature: ReniObject
    {
        private static ConversionFeature _instance;
        private readonly bool _isUseConverter;
        private readonly bool _isDisableCut;

        private ConversionFeature(bool isUseConverter, bool isDisableCut)
        {
            _isUseConverter = isUseConverter;
            _isDisableCut = isDisableCut;
        }

        internal ConversionFeature EnableCut()
        {
            return new ConversionFeature(IsUseConverter, false);
        }

        internal ConversionFeature DontUseConverter()
        {
            return new ConversionFeature(false, IsDisableCut);
        }

        internal bool IsDisableCut { get { return _isDisableCut; } }
        internal bool IsUseConverter { get { return _isUseConverter; } }

        internal static ConversionFeature Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ConversionFeature(true,true);
                return _instance;
            }
        }

    }

    sealed internal class EnableCut: TagChild
    {
        public EnableCut(Base parent)
            : base(parent)
        {
        }

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
        internal override bool IsConvertableToVirt(Base dest, ConversionFeature conversionFeature)
        {
            return base.IsConvertableToVirt(dest, conversionFeature.EnableCut());
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal class Pending: Base
    {
        /// <summary>
        /// The size of type
        /// </summary>
        public override Size Size { get { return Reni.Size.Pending; } }

        /// <summary>
        /// Gets the dump print text.
        /// </summary>
        /// <value>The dump print text.</value>
        /// created 08.01.2007 17:54
        internal override string DumpPrintText { get { return "#(# Prendig type #)#"; } }

        /// <summary>
        /// Converts to.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="dest">The dest.</param>
        /// <returns></returns>
        /// created 11.01.2007 22:12
        internal override Result ConvertToVirt(Category category, Base dest)
        {
            return dest.CreateResult
                (
                category,
                delegate { return Code.Base.Pending; },
                delegate { return Refs.Pending; }
                );
        }

        /// <summary>
        /// Gets a value indicating whether this instance is pending.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is pending; otherwise, <c>false</c>.
        /// </value>
        /// created 09.02.2007 00:26
        internal override bool IsPending { get { return true; } }

        /// <summary>
        /// Visits as sequence.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="elementType">Type of the element.</param>
        /// <returns></returns>
        /// created 13.01.2007 22:20
        internal override Result VisitAsSequence(Category category, Base elementType)
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
            return true;
        }
    }
}

