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

namespace Reni.Type
{
    [Serializable]
    internal abstract class TypeBase : ReniObject, IDumpShortProvider, IIconKeyProvider
    {
        private class Cache
        {
            public static readonly Bit Bit = new Bit();
            public static readonly Void Void = new Void();
            public readonly DictionaryEx<int, Aligner> Aligners;
            public readonly DictionaryEx<int, Array> Arrays;
            public readonly DictionaryEx<int, Sequence> Sequences;
            public readonly DictionaryEx<TypeBase, Pair> Pairs;
            public readonly DictionaryEx<RefAlignParam, Reference> References;
            public readonly SimpleCache<TypeType> TypeType;
            public readonly DictionaryEx<IFunctionalFeature, FunctionAccessType> FunctionalTypes;
            public readonly DictionaryEx<RefAlignParam,ObjectRef> ObjectRefs;
            public readonly DictionaryEx<Struct.Context, DictionaryEx<int, Struct.Reference>> StructReferences;

            public Cache(TypeBase parent)
            {
                StructReferences = new DictionaryEx<Struct.Context, DictionaryEx<int, Struct.Reference>>(context=> new DictionaryEx<int, Struct.Reference>(position=>new Struct.Reference(context,position)));
                ObjectRefs = new DictionaryEx<RefAlignParam, ObjectRef>(refAlignParam=>new ObjectRef(parent, refAlignParam));
                FunctionalTypes = new DictionaryEx<IFunctionalFeature, FunctionAccessType>(feature => new FunctionAccessType(parent, feature));
                References = new DictionaryEx<RefAlignParam, Reference>(refAlignParam => new Reference(parent, refAlignParam));
                Pairs = new DictionaryEx<TypeBase, Pair>(first => new Pair(first, parent));
                Sequences = new DictionaryEx<int, Sequence>(elementCount => new Sequence(parent, elementCount));
                Arrays = new DictionaryEx<int, Array>(count => new Array(parent, count));
                Aligners = new DictionaryEx<int, Aligner>(alignBits => new Aligner(parent, alignBits));
                TypeType = new SimpleCache<TypeType>(() => new TypeType(parent));
            }
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

        internal virtual int GetSequenceCount(TypeBase elementType)
        {
            NotImplementedMethod(elementType);
            return 0;
        }

        [DumpData(false)]
        protected internal virtual int IndexSize { get { return 0; } }

        internal TypeBase CreateAlign(int alignBits)
        {
            if(Size.Align(alignBits) == Size)
                return this;
            return _cache.Aligners.Find(alignBits);
        }

        internal Array CreateArray(int count) { return _cache.Arrays.Find(count); }
        protected virtual TypeBase CreateReversePair(TypeBase first) { return first._cache.Pairs.Find(this); }
        internal Reference CreateReference(RefAlignParam refAlignParam) { return _cache.References.Find(refAlignParam); }
        internal Struct.Reference CreateReference(Struct.Context context, int position) { return _cache.StructReferences.Find(context).Find(position); }
        internal Sequence CreateSequence(int elementCount)
        {
            return _cache.Sequences.Find(elementCount);
        }
        internal TypeBase CreateFunctionalType(IFunctionalFeature feature)
        {
            return _cache.FunctionalTypes.Find(feature);
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
        internal virtual Result ApplyTypeOperator(Result argResult) { return argResult.Type.Conversion(argResult.CompleteCategory, this).ReplaceArg(argResult); }
        internal Result CreateArgResult(Category category) { return CreateResult(category, CreateArgCode); }
        internal Result CreateResult(Result codeAndRefs) { return CreateResult(codeAndRefs.CompleteCategory, codeAndRefs); }
        internal Result CreateResult(Category category, Func<CodeBase> getCode) { return CreateResult(category, getCode, Refs.None); }
        internal Result GenericDumpPrint(Category category) { return GetSuffixResult(category, new Token()); }
        internal CodeBase CreateArgCode() { return CodeBase.CreateArg(Size); }

        internal Result CreateResult(Category category)
        {
            return CreateResult(category,
                                () => CodeBase.Create(Size, BitsConst.Convert(0).Resize(Size)));
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
            if (dest.IsReferenceTo(this))
                return ConvertToReference(category, ((Reference)dest).RefAlignParam);
            return ConvertTo_Implementation(category, dest);
        }

        private Result ConvertToReference(Category category, RefAlignParam refAlignParam)
        {
            return CreateArgResult(category|Category.Type)
                .CreateLocalReferenceResult(refAlignParam)
                & category;
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
            if (dest.IsReferenceTo(this))
                return true;
            if(conversionFeature.IsUseConverter && HasConverterTo(dest))
                return true;
            return IsConvertableTo_Implementation(dest, conversionFeature.DontUseConverter);
        }

        protected virtual bool IsReferenceTo(TypeBase target) { return false; }

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

        internal virtual TypeBase AccessType(Struct.Context context, int position)
        {
            return CreateReference(context, position);
        }

        internal virtual IFunctionalFeature GetFunctionalFeature() { return null; }

        internal virtual TypeBase StripFunctional()
        {
            NotImplementedMethod();
            return null;
        }

        internal virtual bool IsRefLike(Reference target) { return false; }

        private TypeBase CreateSequenceType(TypeBase elementType) { return elementType.CreateSequence(GetSequenceCount(elementType)); }

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
            bool trace = ObjectId == 4 && defineable.ObjectId == 39 && category.HasCode;
            if(!trace) trace = ObjectId == 4 && defineable.ObjectId == 39 && category.HasCode;
            StartMethodDumpWithBreak(trace, category, defineable);
            var searchResult = SearchDefineable<TFeature>(defineable);
            var feature = searchResult.ConvertToFeature();
            if(feature == null)
                return ReturnMethodDump<Result>(trace, null);

            DumpWithBreak(trace, "feature", feature);
            var result = feature.Apply(category);
            DumpWithBreak(trace, "result", result);
            if (this != feature.DefiningType())
            {
                DumpWithBreak(trace, "feature.DefiningType()", feature.DefiningType());
                var conversion = Conversion(category, feature.DefiningType());
                DumpWithBreak(trace, "conversion", conversion);
                result = result.ReplaceArg(conversion);
            }
            return ReturnMethodDumpWithBreak(trace, result);
        }

        internal Result GetSuffixResult(Category category, Defineable defineable) { return GetUnaryResult<IFeature>(category, defineable); }

        internal Result GetPrefixResult(Category category, Defineable defineable) { return GetUnaryResult<IPrefixFeature>(category, defineable); }

        internal Result CreateDereferencedResult(Category category, RefAlignParam refAlignParam)
        {
            return CreateResult
                (
                    category,
                    () => CodeBase.CreateArg(refAlignParam.RefSize).CreateDereference(refAlignParam, Size)
                );
        }

        internal virtual Struct.Context GetStruct()
        {
            NotImplementedMethod();
            return null;
        }

        virtual internal Result Apply(Category category, Func<Category, Result> right, RefAlignParam refAlignParam)
        {
            NotImplementedMethod(category,right,refAlignParam);
            return null;
        }

        internal Result ConvertToAsRef(Category category, Reference target) { 
            if(IsRefLike(target))
                return target.CreateArgResult(category);

            Result convertedResult = Conversion(category|Category.Type, target.AlignedTarget);
            var destructor = convertedResult.Type.Destructor(category);
            return target.CreateResult(
                category,
                () => CodeBase.CreateInternalRef(target.RefAlignParam, convertedResult.Code, destructor.Code),
                () => convertedResult.Refs + destructor.Refs
                );
        }

        internal Result CreateObjectRefInCode(Category category, RefAlignParam refAlignParam)
        {
            var objectRef = ObjectRef(refAlignParam);
            return CreateReference(refAlignParam)
                .CreateResult(
                category, 
                () => CodeBase.Create(objectRef),
                () => Refs.Create(objectRef)
                );
        }

        internal Result ReplaceObjectRefByArg(Result result, RefAlignParam refAlignParam)
        {
            return result
                .ReplaceAbsolute(ObjectRef(refAlignParam), () => CreateReference(refAlignParam).CreateArgResult(result.CompleteCategory));
        }

        private ObjectRef ObjectRef(RefAlignParam refAlignParam) { return _cache.ObjectRefs.Find(refAlignParam); }
    }

    internal class ObjectRef: ReniObject, IRefInCode
    {
        [DumpData(true)]
        private readonly TypeBase _objectType;
        [DumpData(true)]
        private readonly RefAlignParam _refAlignParam;

        public ObjectRef(TypeBase objectType, RefAlignParam refAlignParam)
        {
            _objectType = objectType;
            _refAlignParam = refAlignParam;
        }

        RefAlignParam IRefInCode.RefAlignParam { get { return _refAlignParam; } }
        bool IRefInCode.IsChildOf(ContextBase contextBase) { return false; }
        string IRefInCode.Dump() { return "ObjectRef("+_objectType.DumpShort()+")"; }
    }
}