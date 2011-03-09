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
using Reni.Parser;
using Reni.TokenClasses;
using Reni.Struct;

namespace Reni.Type
{
    [Serializable]
    internal abstract class TypeBase : ReniObject, IDumpShortProvider, IIconKeyProvider
    {
        private sealed class Cache
        {
            public static readonly Bit Bit = new Bit();
            public static readonly Void Void = new Void();
            public readonly DictionaryEx<int, Aligner> Aligners;
            public readonly DictionaryEx<int, Array> Arrays;
            public readonly DictionaryEx<int, Sequence> Sequences;
            public readonly DictionaryEx<TypeBase, Pair> Pairs;
            public readonly DictionaryEx<RefAlignParam, Reference> References;
            public readonly DictionaryEx<RefAlignParam,ObjectReference> ObjectReferences;
            public readonly DictionaryEx<IFunctionalFeature, FunctionAccessType> FunctionalTypes;
            public readonly DictionaryEx<Struct.Context, DictionaryEx<int, Field>> Fields;

            public Cache(TypeBase parent)
            {
                Fields = new DictionaryEx<Struct.Context, DictionaryEx<int, Field>>(context => new DictionaryEx<int, Field>(position => new Field(context, position)));
                ObjectReferences = new DictionaryEx<RefAlignParam, ObjectReference>(refAlignParam => new ObjectReference(parent, refAlignParam));
                FunctionalTypes = new DictionaryEx<IFunctionalFeature, FunctionAccessType>(feature => new FunctionAccessType(parent, feature));
                References = new DictionaryEx<RefAlignParam, Reference>(refAlignParam => new Reference(parent, refAlignParam));
                Pairs = new DictionaryEx<TypeBase, Pair>(first => new Pair(first, parent));
                Sequences = new DictionaryEx<int, Sequence>(elementCount => new Sequence(parent, elementCount));
                Arrays = new DictionaryEx<int, Array>(count => new Array(parent, count));
                Aligners = new DictionaryEx<int, Aligner>(alignBits => new Aligner(parent, alignBits));
            }
        }

        private readonly Cache _cache;

        [UsedImplicitly]
        private static ReniObject _lastSearchVisitor;

        protected TypeBase(int objectId)
            : base(objectId) { _cache = new Cache(this); }

        protected TypeBase() { _cache = new Cache(this); }

        internal static TypeBase Void { get { return Cache.Void; } }
        internal static TypeBase Bit { get { return Cache.Bit; } }

        [Node]
        internal Size Size { get { return GetSize(); } }

        protected abstract Size GetSize();

        internal virtual bool IsRef(RefAlignParam refAlignParam) { return false; }

        [IsDumpEnabled(false)]
        internal virtual bool IsVoid { get { return false; } }

        [IsDumpEnabled(false)]
        internal virtual Size UnrefSize { get { return Size; } }

        [IsDumpEnabled(false)]
        protected internal virtual TypeBase[] ToList { get { return new[] {this}; } }

        string IDumpShortProvider.DumpShort() { return DumpShort(); }

        internal abstract string DumpShort();

        [IsDumpEnabled(false)]
        internal virtual string DumpPrintText
        {
            get
            {
                NotImplementedMethod();
                return "";
            }
        }

        internal virtual bool HasConverterFromBit()
        {
            NotImplementedMethod();
            return false;
        }

        internal virtual int SequenceCount(TypeBase elementType)
        {
            NotImplementedMethod(elementType);
            return 0;
        }

        [IsDumpEnabled(false)]
        protected internal virtual int IndexSize { get { return 0; } }

        internal TypeBase Align(int alignBits)
        {
            if(Size.Align(alignBits) == Size)
                return this;
            return _cache.Aligners.Find(alignBits);
        }

        internal Array Array(int count) { return _cache.Arrays.Find(count); }
        protected virtual TypeBase ReversePair(TypeBase first) { return first._cache.Pairs.Find(this); }
        internal Reference Reference(RefAlignParam refAlignParam) { return _cache.References.Find(refAlignParam); }
        private Field Field(Struct.Context context, int position) { return _cache.Fields.Find(context).Find(position); }
        internal Sequence Sequence(int elementCount) { return _cache.Sequences.Find(elementCount); }
        internal FunctionAccessType FunctionalType(IFunctionalFeature feature) { return _cache.FunctionalTypes.Find(feature); }
        private ObjectReference ObjectReference(RefAlignParam refAlignParam) { return _cache.ObjectReferences.Find(refAlignParam); }
        internal static TypeBase Number(int bitCount) { return Bit.Sequence(bitCount); }
        internal virtual TypeBase AutomaticDereference() { return this; }
        internal virtual TypeBase Pair(TypeBase second) { return second.ReversePair(this); }
        internal virtual TypeBase TypeForTypeOperator() { return this; }
        private static Result VoidCodeAndRefs(Category category) { return VoidResult(category & (Category.Code | Category.Refs)); }
        internal static Result VoidResult(Category category) { return Void.Result(category); }
        internal virtual Result Destructor(Category category) { return VoidCodeAndRefs(category); }
        internal virtual Result ArrayDestructor(Category category, int count) { return VoidCodeAndRefs(category); }
        internal virtual Result Copier(Category category) { return VoidCodeAndRefs(category); }
        internal virtual Result ArrayCopier(Category category, int count) { return VoidCodeAndRefs(category); }
        internal virtual Result ApplyTypeOperator(Result argResult) { return argResult.Type.Conversion(argResult.CompleteCategory, this).ReplaceArg(argResult); }
        protected virtual TypeBase Dereference() { return null; }
        internal Result ArgResult(Category category) { return Result(category, ArgCode); }
        internal Result Result(Result codeAndRefs) { return Result(codeAndRefs.CompleteCategory, codeAndRefs); }
        internal Result Result(Category category, Func<CodeBase> getCode) { return Result(category, getCode, Refs.None); }
        internal Result GenericDumpPrint(Category category) { return GetSuffixResult(category, new Feature.DumpPrint.Token()); }
        internal CodeBase ArgCode() { return CodeBase.Arg(Size); }

        internal Result AutomaticDereferenceResult(Category category)
        {
            var type = Dereference();
            if (type == null)
                return ArgResult(category);
            return DereferenceResult(category).AutomaticDereference();
        }

        protected virtual Result DereferenceResult(Category category)
        {
            NotImplementedMethod(category);
            return null;
        }

        internal Result Result(Category category)
        {
            return Result(category,
                                () => CodeBase.BitsConst(Size, BitsConst.Convert(0).Resize(Size)));
        }

        internal Result Result(Category category, Result codeAndRefs)
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

        internal Result Result(Category category, Func<CodeBase> getCode, Func<Refs> getRefs)
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
            if(thenType.IsConvertableTo(elseType, ConversionParameter.Instance))
                return elseType;
            if(elseType.IsConvertableTo(thenType, ConversionParameter.Instance))
                return thenType;
            thenType.NotImplementedMethod(elseType);
            return null;
        }

        internal Result Conversion(Category category, TypeBase dest)
        {
            if(category <= (Category.Size | Category.Type))
                return dest.Result(category);
            if(IsConvertableTo(dest, ConversionParameter.Instance))
                return ConvertTo(category, dest);
            NotImplementedMethod(category, dest);
            return null;
        }

        internal Result ConvertTo(Category category, TypeBase dest)
        {
            if(this == dest)
                return ConvertToItself(category);
            if (dest.IsReferenceTo(this))
                return LocalReferenceResult(category, ((Reference)dest).RefAlignParam);
            return ConvertToImplementation(category, dest);
        }

        private Result ConvertToSequence(Category category, TypeBase elementType) { return Conversion(category, CreateSequenceType(elementType)); }

        internal Result ConvertToBitSequence(Category category) { return ConvertToSequence(category, Bit).Align(BitsConst.SegmentAlignBits); }

        protected internal virtual Result ConvertToItself(Category category) { return ArgResult(category); }

        protected virtual Result ConvertToImplementation(Category category, TypeBase dest)
        {
            NotImplementedMethod(category, dest);
            return null;
        }

        internal bool IsConvertableTo(TypeBase dest, ConversionParameter conversionParameter)
        {
            if(this == dest)
                return IsConvertableToItself(conversionParameter);
            if (dest.IsReferenceTo(this))
                return true;
            if(conversionParameter.IsUseConverter && HasConverterTo(dest))
                return true;
            return IsConvertableToImplementation(dest, conversionParameter.DontUseConverter);
        }

        protected virtual bool IsReferenceTo(TypeBase value) { return false; }

        internal virtual bool HasConverterTo(TypeBase dest) { return false; }

        internal virtual bool IsConvertableToImplementation(TypeBase dest, ConversionParameter conversionParameter)
        {
            NotImplementedMethod(dest, conversionParameter);
            return false;
        }

        private bool IsConvertableToItself(ConversionParameter conversionParameter) { return true; }

        /// <summary>
        /// Gets the icon key.
        /// </summary>
        /// <value>The icon key.</value>
        string IIconKeyProvider.IconKey { get { return "Type"; } }

        [IsDumpEnabled(false)]
        internal virtual RefAlignParam[] ReferenceChain { get { return new RefAlignParam[0]; } }

        internal virtual IAccessType AccessType(Struct.Context context, int position)
        {
            return Field(context, position);
        }

        internal virtual IFunctionalFeature FunctionalFeature()
        {
            NotImplementedMethod();
            return null;
        }

        internal virtual TypeBase ObjectType()
        {
            NotImplementedMethod();
            return null;
        }

        protected virtual bool IsRefLike(Reference target) { return false; }

        private TypeBase CreateSequenceType(TypeBase elementType) { return elementType.Sequence(SequenceCount(elementType)); }

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
            var trace = ObjectId == -5 && defineable.ObjectId == 4;// && category.HasCode;
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

        internal Result PrefixResult(Category category, Defineable defineable)
        {
            return GetUnaryResult<IPrefixFeature>(category, defineable);
        }

        internal virtual Struct.Context GetStruct()
        {
            NotImplementedMethod();
            return null;
        }

        internal Result Apply(Category category, Func<Category, Result> right, RefAlignParam refAlignParam)
        {
            bool trace = ObjectId == -325 && category.HasCode;
            StartMethodDumpWithBreak(trace, category, right,refAlignParam);
            var functionalFeature = FunctionalFeature();
            var apply = functionalFeature
                .Apply(category, right(Category.Type).Type, refAlignParam);
            var replaceArg = apply
                .ReplaceArg(right(category));
            var result = replaceArg
                .ReplaceObjectRefByArg(refAlignParam, ObjectType());
            DumpWithBreak(trace, "functionalFeature",functionalFeature,"apply",apply,"replaceArg",replaceArg,"result",result);
            return ReturnMethodDump(trace, result);
        }

        internal Result ConvertToAsRef(Category category, Reference target) { 
            if(IsRefLike(target))
                return target.ArgResult(category);

            return LocalReferenceResult(category, target.RefAlignParam);
        }

        internal Result LocalReferenceResult(Category category, RefAlignParam refAlignParam)
        {
            return Align(refAlignParam.AlignBits)
                .Reference(refAlignParam)
                .Result
                (
                    category,
                    () => LocalReferenceCode(refAlignParam),
                    () => Destructor(Category.Refs).Refs
                );
        }

        internal CodeBase LocalReferenceCode(RefAlignParam refAlignParam)
        {
            return ArgCode()
                .LocalReference(refAlignParam, Destructor(Category.Code).Code);
        }

        internal Result ObjectReferenceInCode(Category category, RefAlignParam refAlignParam)
        {
            var objectRef = ObjectReference(refAlignParam);
            return Reference(refAlignParam)
                .Result(
                category, 
                () => CodeBase.ReferenceInCode(objectRef),
                () => Refs.Create(objectRef)
                );
        }

        internal Result ReplaceObjectReferenceByArg(Result result, RefAlignParam refAlignParam)
        {
            return result
                .ReplaceAbsolute(ObjectReference(refAlignParam), () => LocalReferenceResult(result.CompleteCategory, refAlignParam));
        }

        internal virtual Result ReferenceInCode(Context.Function function, Category category)
        {
            return Reference(function.RefAlignParam)
                .Result
                (
                    category,
                    () => CodeBase.ReferenceInCode(function),
                    () => Refs.Create(function)
                )
                ;
        }
    }

    internal interface IAccessType
    {
        Result Result(Category category);
    }
}