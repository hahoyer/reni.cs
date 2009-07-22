using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.TreeStructure;
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
        private class Cache
        {
            public static readonly Bit Bit = new Bit();
            public static readonly Void Void = new Void();
            public readonly DictionaryEx<int, Aligner> Aligners = new DictionaryEx<int, Aligner>();
            public readonly DictionaryEx<int, Array> Arrays = new DictionaryEx<int, Array>();
            public readonly DictionaryEx<int, Sequence> Sequences = new DictionaryEx<int, Sequence>();
            public readonly DictionaryEx<TypeBase, Pair> Pairs = new DictionaryEx<TypeBase, Pair>();
            public readonly DictionaryEx<RefAlignParam, AutomaticRef> AutomaticRefs = new DictionaryEx<RefAlignParam, AutomaticRef>();
            public readonly DictionaryEx<RefAlignParam, AssignableRef> AssignableRefs = new DictionaryEx<RefAlignParam, AssignableRef>();
            public readonly SimpleCache<TypeType> TypeType;
            public readonly SimpleCache<PostProcessorForType> PostProcessor;

            public Cache(TypeBase parent)
            {
                PostProcessor = new SimpleCache<PostProcessorForType>(() => new PostProcessorForType(parent));
                TypeType = new SimpleCache<TypeType>(() => new TypeType(parent));
            }
        }

        private readonly Cache _cache;

        protected TypeBase(int objectId)
            : base(objectId)
        {
            _cache = new Cache(this);
        }

        protected TypeBase()
        {
            _cache = new Cache(this);
        }

        internal static TypeBase CreateVoid { get { return Cache.Void; } }
        internal static TypeBase CreateBit { get { return Cache.Bit; } }

        [Node]
        internal Size Size { get { return GetSize(); } }

        protected abstract Size GetSize();

        internal virtual bool IsRef(RefAlignParam refAlignParam)
        {
            return false;
        }

        [DumpData(false)]
        internal virtual bool IsVoid { get { return false; } }

        [DumpData(false)]
        internal virtual Size UnrefSize { get { return Size; } }

        [DumpData(false)]
        protected internal virtual TypeBase[] ToList { get { return new[] {this}; } }

        string IDumpShortProvider.DumpShort()
        {
            return DumpShort();
        }

        internal abstract string DumpShort();

        private TypeBase TypeType { get { return _cache.TypeType.Value; } }

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
        protected internal virtual int IndexSize { get { return 0; } }

        [DumpData(false)]
        internal PostProcessorForType PostProcessor { get { return _cache.PostProcessor.Value; } }

        internal TypeBase CreateAlign(int alignBits)
        {
            if(Size.Align(alignBits) == Size)
                return this;
            return _cache.Aligners.Find(alignBits, () => new Aligner(this, alignBits));
        }

        internal Array CreateArray(int count)
        {
            return _cache.Arrays.Find(count, () => new Array(this, count));
        }

        internal static TypeBase CreateNumber(int bitCount)
        {
            return CreateBit.CreateSequence(bitCount);
        }

        internal virtual TypeBase CreatePair(TypeBase second)
        {
            return second.CreateReversePair(this);
        }

        protected virtual TypeBase CreateReversePair(TypeBase first)
        {
            return first._cache.Pairs.Find(this,
                                           () => new Pair(first, this));
        }

        internal virtual AutomaticRef CreateAutomaticRef(RefAlignParam refAlignParam)
        {
            return _cache.AutomaticRefs.Find(refAlignParam, () => new AutomaticRef(this, refAlignParam));
        }

        internal virtual AssignableRef CreateAssignableRef(RefAlignParam refAlignParam)
        {
            return _cache.AssignableRefs.Find(refAlignParam, () => new AssignableRef(this, refAlignParam));
        }

        internal Ref EnsureRef(RefAlignParam refAlignParam)
        {
            if(IsRef(refAlignParam))
                return (Ref) this;
            return CreateAutomaticRef(refAlignParam);
        }

        internal Sequence CreateSequence(int elementCount)
        {
            return _cache.Sequences.Find(elementCount, () => new Sequence(this, elementCount));
        }

        internal virtual Result Destructor(Category category)
        {
            return CreateVoidCodeAndRefs(category);
        }

        internal virtual Result ArrayDestructor(Category category, int count)
        {
            return CreateVoidCodeAndRefs(category);
        }

        internal virtual Result Copier(Category category)
        {
            return CreateVoidCodeAndRefs(category);
        }

        internal virtual Result ArrayCopier(Category category, int count)
        {
            return CreateVoidCodeAndRefs(category);
        }

        internal Result CreateArgResult(Category category)
        {
            return CreateResult(category, CreateArgCode);
        }

        internal CodeBase CreateArgCode()
        {
            return CodeBase.CreateArg(Size);
        }

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
            return result;
        }

        internal Result CreateResult(Category category, Func<CodeBase> getCode)
        {
            return CreateResult(category, getCode, Refs.None);
        }

        internal Result CreateResult(Category category, Func<CodeBase> getCode, Func<Refs> getRefs)
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

        internal static Result CreateVoidCodeAndRefs(Category category)
        {
            return CreateVoidResult(category & (Category.Code | Category.Refs));
        }

        internal static Result CreateVoidResult(Category category)
        {
            return CreateVoid.CreateResult(category);
        }

        internal virtual Result AutomaticDereference(Result result)
        {
            return result;
        }

        internal virtual TypeBase AutomaticDereference()
        {
            return this;
        }

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

        internal Result ArrayDumpPrint(Category category, int count)
        {
            NotImplementedMethod(category, count);
            throw new NotImplementedException();
        }

        internal virtual Result SequenceDumpPrint(Category category, int count)
        {
            NotImplementedMethod(category, count);
            throw new NotImplementedException();
        }

        internal virtual Result ApplyTypeOperator(Result argResult)
        {
            return argResult.Type.Conversion(argResult.CompleteCategory, this).UseWithArg(argResult);
        }

        internal static TypeBase CommonType(TypeBase thenType, TypeBase elseType)
        {
            if(thenType.IsConvertableTo(elseType, ConversionFeature.Instance))
                return elseType;
            if(elseType.IsConvertableTo(thenType, ConversionFeature.Instance))
                return thenType;
            thenType.NotImplementedMethod(elseType);
            throw new NotImplementedException();
        }

        internal Result Conversion(Category category, TypeBase dest)
        {
            if(category <= (Category.Size | Category.Type))
                return dest.CreateResult(category);
            if(IsConvertableTo(dest, ConversionFeature.Instance))
                return ConvertTo(category, dest);
            NotImplementedMethod(category, dest);
            throw new NotImplementedException();
        }

        internal Result ConvertTo(Category category, TypeBase dest)
        {
            if(this == dest)
                return ConvertToItself(category);
            return ConvertToImplementation(category, dest);
        }

        protected virtual Result ConvertToItself(Category category)
        {
            return CreateArgResult(category);
        }

        protected virtual Result ConvertToImplementation(Category category, TypeBase dest)
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
            return IsConvertableToImplementation(dest, conversionFeature);
        }

        internal virtual bool HasConverterTo(TypeBase dest)
        {
            return false;
        }

        internal virtual bool IsConvertableToImplementation(TypeBase dest, ConversionFeature conversionFeature)
        {
            NotImplementedMethod(dest, conversionFeature);
            throw new NotImplementedException();
        }

        private bool IsConvertableToItself(ConversionFeature conversionFeature)
        {
            return true;
        }

        protected virtual CodeBase CreateSequenceOperation(Size size, Defineable token, Size objSize, Size argsSize)
        {
            NotImplementedMethod(size, token, objSize, argsSize);
            return null;
        }

        protected virtual CodeBase CreateSequenceOperation(Size size, Defineable token, Size objSize)
        {
            NotImplementedMethod(size, token, objSize);
            return null;
        }

        protected virtual TypeBase SequenceOperationResultType(Defineable token, int objBitCount, int argBitCount)
        {
            NotImplementedMethod(token, objBitCount, argBitCount);
            return null;
        }

        protected virtual TypeBase SequenceOperationResultType(Defineable token, int objBitCount)
        {
            NotImplementedMethod(token, objBitCount);
            return null;
        }

        internal virtual Result UnProperty(Result rawResult)
        {
            return rawResult;
        }

        private Result SequenceOperationResult(Category category, Defineable definable, Size objSize, Size argsSize)
        {
            var type = SequenceOperationResultType(definable, objSize.ToInt(), argsSize.ToInt());
            return type
                .CreateResult(category, () => CreateSequenceOperation(type.Size, definable, objSize, argsSize));
        }

        private Result SequenceOperationResult(Category category, Defineable definable, Size objSize)
        {
            var type = SequenceOperationResultType(definable, objSize.ToInt());
            return type
                .CreateResult(category, () => CreateSequenceOperation(type.Size, definable, objSize));
        }

        internal virtual Result AccessResultAsArgFromRef(Category category, int position, RefAlignParam refAlignParam)
        {
            NotImplementedMethod(category, position, refAlignParam);
            return null;
        }

        internal virtual Result AccessResultAsContextRefFromRef(Category category, int position, RefAlignParam refAlignParam)
        {
            NotImplementedMethod(category, position, refAlignParam);
            return null;
        }

        internal virtual Result AccessResultAsArg(Category category, int position)
        {
            NotImplementedMethod(category, position);
            return null;
        }

        internal Result CreateAssignableRefResult(Category category, RefAlignParam refAlignParam, Func<CodeBase> getCode, Func<Refs> getRefs)
        {
            if(Size.IsZero)
                return CreateResult(category);

            return CreateAssignableRef(refAlignParam).CreateResult(category, getCode, getRefs);
        }

        /// <summary>
        /// Gets the icon key.
        /// </summary>
        /// <value>The icon key.</value>
        string IIconKeyProvider.IconKey { get { return "Type"; } }

        internal virtual bool IsRefLike(Ref target)
        {
            return false;
        }

        internal Result ApplySequenceOperation(SequenceOfBitOperation definable, ContextBase callContext, Category category, ICompileSyntax @object)
        {
            var result = SequenceOperationResult
                (
                category,
                definable,
                callContext.Type(@object).UnrefSize
                );

            var objectResult = callContext.ConvertToSequence(category, @object, this);

            return result.UseWithArg(objectResult);
        }

        internal Result ApplySequenceOperation(SequenceOfBitOperation definable, ContextBase callContext,
                                               Category category, ICompileSyntax @object, ICompileSyntax args)
        {
            var result = SequenceOperationResult
                (
                category,
                definable,
                callContext.Type(@object).UnrefSize,
                callContext.Type(args).UnrefSize
                );

            var argsResult = callContext.ConvertToSequence(category, args, this);
            var objectResult = callContext.ConvertToSequence(category, @object, this);

            return result.UseWithArg(objectResult.CreateSequence(argsResult));
        }

        internal TypeBase CreateSequenceType(TypeBase elementType)
        {
            return elementType.CreateSequence(SequenceCount);
        }

        internal IFeature SearchDefineable(DefineableToken defineableToken)
        {
            var searchVisitor = new RootSearchVisitor<IFeature>(defineableToken.TokenClass);
            searchVisitor.Search(this);
            return searchVisitor.Result;
        }

        internal IPrefixFeature SearchDefineablePrefix(DefineableToken defineableToken)
        {
            var searchVisitor = new RootSearchVisitor<IPrefixFeature>(defineableToken.TokenClass);
            searchVisitor.Search(this);
            return searchVisitor.Result;
        }

        internal virtual void Search(ISearchVisitor searchVisitor)
        {
            searchVisitor.SearchTypeBase();
        }

        internal virtual Result ArrayDumpPrintFromRef(Category category, int count, RefAlignParam refAlignParam)
        {
            NotImplementedMethod(category, count, refAlignParam);
            return null;
        }
    }

}