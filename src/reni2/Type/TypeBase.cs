using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Parser;
using Reni.Parser.TokenClass;
using Reni.Syntax;

namespace Reni.Type
{
    [Serializable]
    internal abstract class TypeBase : ReniObject, IDumpShortProvider, IIconKeyProvider
    {
        private static readonly Bit _bit = new Bit();
        private static readonly Void _void = new Void();
        private static readonly Pending _pending = new Pending();
        private readonly DictionaryEx<int, Aligner> _aligner = new DictionaryEx<int, Aligner>();
        private readonly DictionaryEx<int, Array> _array = new DictionaryEx<int, Array>();
        private readonly DictionaryEx<int, Sequence> _chain = new DictionaryEx<int, Sequence>();
        private readonly DictionaryEx<TypeBase, Pair> _pair = new DictionaryEx<TypeBase, Pair>();
        private readonly DictionaryEx<RefAlignParam, AutomaticRef> _ref = new DictionaryEx<RefAlignParam, AutomaticRef>();
        private readonly DictionaryEx<RefAlignParam, AssignableRef> _assignableRef = new DictionaryEx<RefAlignParam, AssignableRef>();
        private readonly SimpleCache<TypeType> _typeTypeCache = new SimpleCache<TypeType>();
        private readonly SimpleCache<PostProcessorForType> _postProcessor = new SimpleCache<PostProcessorForType>();

        protected TypeBase(int objectId)
            : base(objectId) { }

        protected TypeBase() { }

        internal static TypeBase CreateVoid { get { return _void; } }
        internal static TypeBase CreateBit { get { return _bit; } }
        internal static TypeBase Pending { get { return _pending; } }

        [Node]
        internal Size Size { get { return GetSize(); } }

        protected abstract Size GetSize();

        internal virtual bool IsRef(RefAlignParam refAlignParam) { return false; }

        [DumpData(false)]
        internal virtual bool IsVoid { get { return false; } }
        [DumpData(false)]
        internal virtual Size UnrefSize { get { return Size; } }
        [DumpData(false)]
        internal virtual bool IsPending { get { return false; } }
        [DumpData(false)]
        internal protected virtual TypeBase[] ToList { get { return new[] {this}; } }

        string IDumpShortProvider.DumpShort() { return DumpShort(); }

        internal abstract string DumpShort();

        private TypeBase TypeType { get { return _typeTypeCache.Find(() => new TypeType(this)); } }

        [DumpData(false)]
        internal virtual string DumpPrintText
        {
            get
            {
                NotImplementedMethod();
                throw new NotImplementedException();
            }
        }

        internal virtual bool HasConverterFromBit()
        {
            NotImplementedMethod();
            throw new NotImplementedException();
        }

        [DumpData(false)]
        internal virtual int SequenceCount
        {
            get
            {
                NotImplementedMethod();
                return 0;
            }
        }

        [DumpData(false)]
        internal TypeBase IndexType { get { return CreateNumber(IndexSize); } }

        [DumpData(false)]
        internal protected virtual int IndexSize { get { return 0; } }

        [DumpData(false)]
        internal PostProcessorForType PostProcessor { get { return _postProcessor.Find(() => new PostProcessorForType(this)); } }

        internal TypeBase CreateAlign(int alignBits)
        {
            if(Size.Align(alignBits) == Size)
                return this;
            return _aligner.Find(alignBits, () => new Aligner(this, alignBits));
        }

        internal Array CreateArray(int count) { return _array.Find(count, () => new Array(this, count)); }

        internal static TypeBase CreateNumber(int bitCount) { return CreateBit.CreateSequence(bitCount); }

        internal virtual TypeBase CreatePair(TypeBase second) { return second.CreateReversePair(this); }

        protected virtual TypeBase CreateReversePair(TypeBase first)
        {
            return first._pair.Find(this,
                () => new Pair(first, this));
        }

        internal virtual AutomaticRef CreateAutomaticRef(RefAlignParam refAlignParam) { return _ref.Find(refAlignParam, () => new AutomaticRef(this, refAlignParam)); }

        internal virtual AssignableRef CreateAssignableRef(RefAlignParam refAlignParam) { return _assignableRef.Find(refAlignParam, () => new AssignableRef(this, refAlignParam)); }

        internal Ref EnsureRef(RefAlignParam refAlignParam)
        {
            if(IsRef(refAlignParam))
                return (Ref) this;
            return CreateAutomaticRef(refAlignParam);
        }

        internal Sequence CreateSequence(int elementCount) { return _chain.Find(elementCount, () => new Sequence(this, elementCount)); }

        internal virtual Result DestructorHandler(Category category) { return EmptyHandler(category); }

        internal virtual Result ArrayDestructorHandler(Category category, int count) { return EmptyHandler(category); }

        internal virtual Result MoveHandler(Category category) { return EmptyHandler(category); }

        internal virtual Result ArrayMoveHandler(Category category, int count) { return EmptyHandler(category); }

        internal Result CreateArgResult(Category category) { return CreateResult(category, CreateArgCode); }

        internal CodeBase CreateArgCode() { return CodeBase.CreateArg(Size); }

        internal Result CreateResult(Category category)
        {
            return CreateResult(category,
                () => CodeBase.CreateBitArray(Size, BitsConst.Convert(0).Resize(Size)));
        }

        internal Result CreateResult(Category category, Result codeAndRefs)
        {
            var result = new Result();
            if(category.HasSize)
                result.Size = Size;
            if(category.HasType)
                result.Type = this;
            if(category.HasCode)
                result.Code = codeAndRefs.Code;
            if(category.HasRefs)
                result.Refs = codeAndRefs.Refs;
            if(category.HasInternal)
                result.Internal = codeAndRefs.Internal;
            return result;
        }

        internal Result CreateResult(Category category, Func<CodeBase> getCode) { return CreateResult(category, getCode, Refs.None, Result.EmptyInternal); }
        internal Result CreateResult(Category category, Func<CodeBase> getCode, Func<Result> getInternal) { return CreateResult(category, getCode, Refs.None, getInternal); }
        internal Result CreateResult(Category category, Func<CodeBase> getCode, Func<Refs> getRefs) { return CreateResult(category, getCode, getRefs, Result.EmptyInternal); }

        internal Result CreateResult(Category category, Func<CodeBase> getCode, Func<Refs> getRefs, Func<Result> getInternal)
        {
            var result = new Result();
            if(category.HasSize)
                result.Size = Size;
            if(category.HasType)
                result.Type = this;
            if(category.HasCode)
                result.Code = getCode();
            if(category.HasRefs)
                result.Refs = getRefs();
            if(category.HasInternal)
                result.Internal = getInternal();
            return result;
        }

        internal virtual Result ApplyFunction(Category category, ContextBase callContext, ICompileSyntax args)
        {
            NotImplementedMethod(callContext, category, args);
            throw new NotImplementedException();
        }

        internal virtual Result ApplyFunction(Category category, Result argsResult)
        {
            NotImplementedMethod(category, argsResult);
            return null;
        }

        internal static Result EmptyHandler(Category category) { return CreateVoidResult(category - Category.Type - Category.Size); }

        internal static Result CreateVoidResult(Category category) { return CreateVoid.CreateResult(category); }

        internal virtual Result AutomaticDereference(Result result) { return result; }

        internal virtual TypeBase AutomaticDereference() { return this; }

        internal virtual Result TypeOperator(Category category)
        {
            var result = CreateVoidResult(category).Clone();
            if(category.HasType)
                result.Type = TypeType;
            return result;
        }

        internal virtual Result DumpPrint(Category category)
        {
            NotImplementedMethod(category);
            throw new NotImplementedException();
        }

        internal virtual Result DumpPrintFromRef(Category category, RefAlignParam refAlignParam)
        {
            var argResult = CreateAutomaticRef(refAlignParam).Conversion(category, this);
            return DumpPrint(category).UseWithArg(argResult);
        }

        internal virtual Result ArrayDumpPrint(Category category, int count)
        {
            NotImplementedMethod(category, count);
            throw new NotImplementedException();
        }

        internal virtual Result SequenceDumpPrint(Category category, int count)
        {
            NotImplementedMethod(category, count);
            throw new NotImplementedException();
        }

        internal virtual TypeBase DumpPrintArrayType(int count)
        {
            NotImplementedMethod(count);
            throw new NotImplementedException();
        }

        internal Result ApplyTypeOperator(Category category, TypeBase targetType) { return targetType.Conversion(category, this); }

        internal virtual Result ApplyTypeOperator(Result argResult) { return argResult.Type.Conversion(argResult.Complete, this).UseWithArg(argResult); }

        internal TypeBase CommonType(TypeBase dest)
        {
            if(IsConvertableTo(dest, ConversionFeature.Instance))
                return dest;
            if(dest.IsConvertableTo(this, ConversionFeature.Instance))
                return this;
            NotImplementedMethod(dest);
            throw new NotImplementedException();
        }

        internal Result Conversion(Category category, TypeBase dest)
        {
            if(category.HasCode || category.HasRefs)
            {
                if(IsConvertableTo(dest, ConversionFeature.Instance))
                    return ConvertTo(category, dest);
                NotImplementedMethod(category, dest);
                throw new NotImplementedException();
            }
            return dest.CreateResult(category);
        }

        internal Result ConvertTo(Category category, TypeBase dest)
        {
            if(this == dest)
                return ConvertToItself(category);
            return ConvertToVirt(category, dest);
        }

        internal virtual Result ConvertToItself(Category category) { return CreateArgResult(category); }

        internal virtual Result ConvertToVirt(Category category, TypeBase dest)
        {
            NotImplementedMethod(category, dest);
            throw new NotImplementedException();
        }

        internal bool IsConvertableTo(TypeBase dest, ConversionFeature conversionFeature)
        {
            if(this == dest)
                return IsConvertableToItself(conversionFeature);
            if(conversionFeature.IsUseConverter && HasConverterTo(dest))
                return true;
            return IsConvertableToVirt(dest, conversionFeature);
        }

        internal virtual bool HasConverterTo(TypeBase dest) { return false; }

        internal virtual bool IsConvertableToVirt(TypeBase dest, ConversionFeature conversionFeature)
        {
            NotImplementedMethod(dest, conversionFeature);
            throw new NotImplementedException();
        }

        internal virtual bool IsConvertableToItself(ConversionFeature conversionFeature) { return true; }

        internal virtual CodeBase CreateSequenceOperation(Size size, Defineable token, Size objSize, Size argsSize)
        {
            NotImplementedMethod(size, token, objSize, argsSize);
            return null;
        }

        internal virtual CodeBase CreateSequenceOperation(Size size, Defineable token, Size objSize)
        {
            NotImplementedMethod(size, token, objSize);
            return null;
        }

        internal protected virtual TypeBase SequenceOperationResultType(Defineable token, int objBitCount, int argBitCount)
        {
            NotImplementedMethod(token, objBitCount, argBitCount);
            return null;
        }

        internal protected virtual TypeBase SequenceOperationResultType(Defineable token, int objBitCount)
        {
            NotImplementedMethod(token, objBitCount);
            return null;
        }

        internal Result ThenElseWithPending(Category category, Refs condRefs, Refs elseOrThenRefs)
        {
            Tracer.Assert(!category.HasCode);

            return CreateResult
                (
                category,
                () => null,
                () => condRefs.Pair(elseOrThenRefs)
                );
        }

        internal virtual CodeBase CreateRefCodeForContext(ContextBase context)
        {
            NotImplementedMethod(context);
            return null;
        }

        internal virtual Result UnProperty(Result rawResult) { return rawResult; }

        internal virtual TypeBase UnProperty() { return this; }

        internal SearchResult<IFeature> SearchDefineable(DefineableToken defineableToken) { return Search(defineableToken.TokenClass).SubTrial(this); }
        internal SearchResult<IPrefixFeature> SearchDefineablePrefix(DefineableToken defineableToken) { return SearchPrefix(defineableToken.TokenClass).SubTrial(this); }

        internal virtual SearchResult<IFeature> Search(Defineable defineable) { return defineable.Search().SubTrial(this); }
        internal virtual SearchResult<IPrefixFeature> SearchPrefix(Defineable defineable) { return defineable.SearchPrefix().SubTrial(this); }
        internal virtual SearchResult<IConverter<IFeature, Ref>> SearchFromRef(Defineable defineable) { return defineable.SearchFromRef().SubTrial(this); }
        internal virtual SearchResult<IConverter<IFeature, Sequence>> SearchFromSequence(Defineable defineable) { return defineable.SearchFromSequenceElement().SubTrial(this); }
        internal virtual SearchResult<IConverter<IPrefixFeature, Sequence>> SearchPrefixFromSequence(Defineable defineable) { return defineable.SearchPrefixFromSequenceElement().SubTrial(this); }
        internal virtual SearchResult<IConverter<IConverter<IFeature, Ref>, Sequence>> SearchFromRefToSequence(Defineable defineable) { return SearchResult<IConverter<IConverter<IFeature, Ref>, Sequence>>.Failure(this, defineable); }

        internal Result ConvertToSequence(ContextBase callContext, Category category, ICompileSyntax args)
        {
            var result = callContext.Result(category | Category.Type, args);
            return result.ConvertTo(CreateSequence(result.Type.SequenceCount));
        }

        internal Result SequenceOperationResult(Category category, Defineable definable, Size objSize, Size argsSize)
        {
            var type = SequenceOperationResultType(definable, objSize.ToInt(), argsSize.ToInt());
            return type
                .CreateResult(category, () => CreateSequenceOperation(type.Size, definable, objSize, argsSize));
        }

        internal Result SequenceOperationResult(Category category, Defineable definable, Size objSize)
        {
            var type = SequenceOperationResultType(definable, objSize.ToInt());
            return type
                .CreateResult(category, () => CreateSequenceOperation(type.Size, definable, objSize));
        }

        internal virtual Result AccessResultFromRef(Category category, int position, RefAlignParam refAlignParam)
        {
            NotImplementedMethod(category, position, refAlignParam);
            return null;
        }

        internal virtual Result AccessResult(Category category, int position)
        {
            NotImplementedMethod(category, position);
            return null;
        }

        internal Result CreateAssignableRefResult(Category category, RefAlignParam refAlignParam, Func<CodeBase> getCode)
        {
            if(Size.IsZero)
                return CreateResult(category);

            return CreateAssignableRef(refAlignParam).CreateResult(category, getCode);
        }

        /// <summary>
        /// Gets the icon key.
        /// </summary>
        /// <value>The icon key.</value>
        string IIconKeyProvider.IconKey { get { return "Type"; } }
    }
}