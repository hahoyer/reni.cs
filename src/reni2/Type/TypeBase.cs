using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.TreeStructure;
using JetBrains.Annotations;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Feature.DumpPrint;
using Reni.Parser.TokenClass;
using Reni.Struct;
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

            public readonly DictionaryEx<RefAlignParam, AutomaticRef> AutomaticRefs =
                new DictionaryEx<RefAlignParam, AutomaticRef>();

            public readonly SimpleCache<TypeType> TypeType;

            public readonly DictionaryEx<IFunctionalFeature, FunctionAccessType> FunctionalTypes =
                new DictionaryEx<IFunctionalFeature, FunctionAccessType>();

            public Cache(TypeBase parent) { TypeType = new SimpleCache<TypeType>(() => new TypeType(parent)); }
        }

        private readonly Cache _cache;

        [UsedImplicitly]
        private static ReniObject _lastSearchVisitor;

        protected TypeBase(int objectId)
            : base(objectId) { _cache = new Cache(this); }

        protected TypeBase() { _cache = new Cache(this); }

        internal static TypeBase CreateVoid { get { return Cache.Void; } }
        internal static TypeBase CreateBit { get { return Cache.Bit; } }

        [Node]
        internal Size Size { get { return GetSize(); } }

        protected abstract Size GetSize();

        internal virtual bool IsRef(RefAlignParam refAlignParam) { return false; }

        [DumpData(false)]
        internal virtual bool IsVoid { get { return false; } }

        [DumpData(false)]
        internal virtual Size UnrefSize { get { return Size; } }

        [DumpData(false)]
        protected internal virtual TypeBase[] ToList { get { return new[] {this}; } }

        string IDumpShortProvider.DumpShort() { return DumpShort(); }

        internal abstract string DumpShort();

        [DumpData(false)]
        internal TypeType TypeType { get { return _cache.TypeType.Value; } }

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
        protected internal virtual int IndexSize { get { return 0; } }

        internal TypeBase CreateAlign(int alignBits)
        {
            if(Size.Align(alignBits) == Size)
                return this;
            return _cache
                .Aligners
                .Find(alignBits, () => new Aligner(this, alignBits));
        }

        internal Array CreateArray(int count)
        {
            return _cache
                .Arrays
                .Find(count, () => new Array(this, count));
        }

        protected virtual TypeBase CreateReversePair(TypeBase first)
        {
            return first
                ._cache
                .Pairs
                .Find(this, () => new Pair(first, this));
        }

        internal virtual AutomaticRef CreateAutomaticRef(RefAlignParam refAlignParam)
        {
            return _cache
                .AutomaticRefs
                .Find(refAlignParam, () => new AutomaticRef(this, refAlignParam));
        }

        internal Sequence CreateSequence(int elementCount)
        {
            return _cache
                .Sequences
                .Find(elementCount, () => new Sequence(this, elementCount));
        }

        internal TypeBase CreateFunctionalType(IFunctionalFeature feature)
        {
            return _cache
                .FunctionalTypes
                .Find(feature, () => new FunctionAccessType(this, feature));
        }

        internal static TypeBase CreateNumber(int bitCount) { return CreateBit.CreateSequence(bitCount); }
        internal virtual TypeBase AutomaticDereference() { return this; }
        internal virtual TypeBase CreatePair(TypeBase second) { return second.CreateReversePair(this); }
        internal virtual TypeBase GetEffectiveType() { return this; }
        internal static Result CreateVoidCodeAndRefs(Category category) { return CreateVoidResult(category & (Category.Code | Category.Refs)); }
        internal static Result CreateVoidResult(Category category) { return CreateVoid.CreateResult(category); }
        internal virtual Result Destructor(Category category) { return CreateVoidCodeAndRefs(category); }
        internal virtual Result ArrayDestructor(Category category, int count) { return CreateVoidCodeAndRefs(category); }
        internal virtual Result Copier(Category category) { return CreateVoidCodeAndRefs(category); }
        internal virtual Result ArrayCopier(Category category, int count) { return CreateVoidCodeAndRefs(category); }
        internal virtual Result AutomaticDereference(Result result) { return result; }
        internal virtual Result ApplyTypeOperator(Result argResult) { return argResult.Type.Conversion(argResult.CompleteCategory, this).UseWithArg(argResult); }
        internal Result CreateArgResult(Category category) { return CreateResult(category, CreateArgCode); }
        internal Result CreateResult(Result codeAndRefs) { return CreateResult(codeAndRefs.CompleteCategory, codeAndRefs); }
        internal Result CreateResult(Category category, Func<CodeBase> getCode) { return CreateResult(category, getCode, Refs.None); }
        internal Result GenericDumpPrint(Category category) { return GetUnaryResult<IFeature>(category, new Token()); }
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
            return result;
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
            return ConvertTo_Implementation(category, dest);
        }

        private Result ConvertToSequence(Category category, TypeBase elementType) { return Conversion(category, CreateSequenceType(elementType)); }

        internal Result ConvertToBitSequence(Category category) { return ConvertToSequence(category, CreateBit).Align(BitsConst.SegmentAlignBits); }

        protected internal virtual Result ConvertToItself(Category category) { return CreateArgResult(category); }

        protected virtual Result ConvertTo_Implementation(Category category, TypeBase dest)
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
            return IsConvertableTo_Implementation(dest, conversionFeature.DontUseConverter);
        }

        internal virtual bool HasConverterTo(TypeBase dest) { return false; }

        internal virtual bool IsConvertableTo_Implementation(TypeBase dest, ConversionFeature conversionFeature)
        {
            NotImplementedMethod(dest, conversionFeature);
            throw new NotImplementedException();
        }

        private bool IsConvertableToItself(ConversionFeature conversionFeature) { return true; }

        /// <summary>
        /// Gets the icon key.
        /// </summary>
        /// <value>The icon key.</value>
        string IIconKeyProvider.IconKey { get { return "Type"; } }

        internal abstract bool IsValidRefTarget();

        internal virtual IFunctionalFeature GetFunctionalFeature() { return null; }

        internal virtual TypeBase StripFunctional()
        {
            NotImplementedMethod();
            return null;
        }

        internal virtual bool IsRefLike(Ref target) { return false; }

        private TypeBase CreateSequenceType(TypeBase elementType) { return elementType.CreateSequence(SequenceCount); }

        private TFeature SearchDefineable<TFeature>(Defineable defineable)
            where TFeature : class
        {
            var searchVisitor = new RootSearchVisitor<TFeature>(defineable);
            searchVisitor.Search(this);
            if(Debugger.IsAttached)
                _lastSearchVisitor = searchVisitor;
            return searchVisitor.Result;
        }

        internal virtual void Search(ISearchVisitor searchVisitor) { searchVisitor.Search(); }

        private Result GetUnaryResult<TFeature>(Category category, Defineable defineable)
            where TFeature : class
        {
            var searchResult = SearchDefineable<TFeature>(defineable);
            var feature = searchResult.Feature();
            if(feature == null)
                return null;

            var result = feature.Apply(category);
            if(this != feature.DefiningType())
            {
                var conversion = Conversion(category, feature.DefiningType());
                result = result.UseWithArg(conversion);
            }
            return result & category;
        }

        internal Result GetSuffixResult(Category category, Defineable defineable) { return GetUnaryResult<IFeature>(category, defineable); }

        internal Result GetPrefixResult(Category category, Defineable defineable) { return GetUnaryResult<IPrefixFeature>(category, defineable); }

        internal Result AtResult(ContextBase callContext, Category category, ICompileSyntax right)
        {
            var thisTypeResult = Conversion(category | Category.Type, GetThisType());
            var thisType = (ThisType) thisTypeResult.Type;
            var position = callContext.Evaluate(right, thisType.IndexType).ToInt32();
            return thisType
                .AccessResult(category, position)
                .UseWithArg(thisTypeResult);
        }

        protected virtual ThisType GetThisType()
        {
            NotImplementedMethod();
            return null;
        }

        internal void ChildSearch(ISearchVisitor searchVisitor, StructRef structRef)
        {
            Search(searchVisitor.Child(structRef));
            Search(searchVisitor);
        }

        internal Result DumpPrintFromReference(Category category, Result referenceResult, RefAlignParam refAlignParam)
        {
            var dereferencedResult = DereferencedResult(category, referenceResult, refAlignParam);
            return GenericDumpPrint(category).UseWithArg(dereferencedResult);
        }

        private Result DereferencedResult(Category category, Result referenceResult, RefAlignParam refAlignParam)
        {
            return CreateResult
                (
                    category,
                    () => referenceResult.Code.CreateDereference(refAlignParam, Size),
                    () => referenceResult.Refs
                );
        }
    }
}