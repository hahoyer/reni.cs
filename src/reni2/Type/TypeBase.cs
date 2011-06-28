using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.TreeStructure;
using JetBrains.Annotations;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Sequence;
using Reni.Struct;
using Reni.TokenClasses;

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
            public readonly DictionaryEx<int, BaseType> Sequences;
            public readonly DictionaryEx<TypeBase, Pair> Pairs;
            public readonly DictionaryEx<RefAlignParam, AutomaticReferenceType> References;
            public readonly DictionaryEx<RefAlignParam, ObjectReference> ObjectReferences;
            public readonly SimpleCache<TypeType> TypeType;
            public readonly SimpleCache<Context.Function> Function;

            public readonly DictionaryEx<Structure, DictionaryEx<int, AccessType>> AccessTypes;

            public Cache(TypeBase parent)
            {
                AccessTypes = new DictionaryEx<Structure, DictionaryEx<int, AccessType>>(
                    accessPoint => new DictionaryEx<int, AccessType>(
                        position => new AccessType(parent, accessPoint, position)
                    )
                );
                ObjectReferences = new DictionaryEx<RefAlignParam, ObjectReference>(refAlignParam => new ObjectReference(parent, refAlignParam));
                References = new DictionaryEx<RefAlignParam, AutomaticReferenceType>(parent.ObtainReference);
                Pairs = new DictionaryEx<TypeBase, Pair>(first => new Pair(first, parent));
                Sequences = new DictionaryEx<int, BaseType>(elementCount => new BaseType(parent, elementCount));
                Arrays = new DictionaryEx<int, Array>(count => new Array(parent, count));
                Aligners = new DictionaryEx<int, Aligner>(alignBits => new Aligner(parent, alignBits));
                TypeType = new SimpleCache<TypeType>(() => new TypeType(parent));
                Function = new SimpleCache<Context.Function>(() => new Context.Function(parent));
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

        [DisableDump]
        internal virtual bool IsVoid { get { return false; } }

        [DisableDump]
        internal virtual Size UnrefSize { get { return Size; } }

        [DisableDump]
        protected internal virtual TypeBase[] ToList { get { return new[] {this}; } }

        string IDumpShortProvider.DumpShort() { return DumpShort(); }

        [DisableDump]
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

        [DisableDump]
        protected internal virtual int IndexSize { get { return 0; } }

        internal TypeBase Align(int alignBits)
        {
            if(Size.Align(alignBits) == Size)
                return this;
            return _cache.Aligners.Find(alignBits);
        }

        internal Array SpawnArray(int count) { return _cache.Arrays.Find(count); }
        protected virtual TypeBase ReversePair(TypeBase first) { return first._cache.Pairs.Find(this); }
        internal virtual AutomaticReferenceType SpawnReference(RefAlignParam refAlignParam) { return _cache.References.Find(refAlignParam); }
        internal BaseType SpawnSequence(int elementCount) { return _cache.Sequences.Find(elementCount); }
        protected ObjectReference ObjectReference(RefAlignParam refAlignParam) { return _cache.ObjectReferences.Find(refAlignParam); }
        internal static TypeBase Number(int bitCount) { return Bit.SpawnSequence(bitCount); }
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
        internal CodeBase ArgCode() { return CodeBase.Arg(this); }

        internal Result AutomaticDereferenceResult(Category category)
        {
            var type = Dereference();
            if(type == null)
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
            return Result
                (
                    category,
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
            if(dest.IsReferenceTo(this))
                return LocalReferenceResult(category, ((AutomaticReferenceType) dest).RefAlignParam);
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
            if(dest.IsReferenceTo(this))
                return true;
            if(conversionParameter.IsUseConverter && HasConverterTo(dest))
                return true;
            return IsConvertableToImplementation(dest, conversionParameter.DontUseConverter);
        }

        protected virtual bool IsReferenceTo(TypeBase value) { return false; }

        internal virtual bool HasConverterTo(TypeBase dest) { return false; }

        internal virtual bool IsConvertableToImplementation
            (TypeBase dest, ConversionParameter conversionParameter)
        {
            NotImplementedMethod(dest, conversionParameter);
            return false;
        }

        private bool IsConvertableToItself(ConversionParameter conversionParameter) { return true; }

        /// <summary>
        ///     Gets the icon key.
        /// </summary>
        /// <value>The icon key.</value>
        string IIconKeyProvider.IconKey { get { return "Type"; } }

        [DisableDump]
        internal virtual RefAlignParam[] ReferenceChain { get { return new RefAlignParam[0]; } }

        [DisableDump]
        internal TypeType TypeType { get { return _cache.TypeType.Value; } }

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

        protected virtual bool IsRefLike(AutomaticReferenceType target) { return false; }

        private TypeBase CreateSequenceType(TypeBase elementType) { return elementType.SpawnSequence(SequenceCount(elementType)); }

        internal TFeature SearchDefineable<TFeature>(Defineable defineable)
            where TFeature : class
        {
            var searchVisitor = new RootSearchVisitor<TFeature>(defineable);
            searchVisitor.Search(this);
            if(Debugger.IsAttached)
                _lastSearchVisitor = searchVisitor;
            return searchVisitor.Result;
        }

        internal virtual void Search(ISearchVisitor searchVisitor) { searchVisitor.Search(); }

        internal virtual Structure GetStructure()
        {
            NotImplementedMethod();
            return null;
        }

        internal Result Apply(Category category, Result rightResult, RefAlignParam refAlignParam)
        {
            var trace = ObjectId == -10 && category.HasType;
            StartMethodDumpWithBreak(trace, category, rightResult, refAlignParam);
            var functionalFeature = FunctionalFeature();
            var apply = functionalFeature.Apply(category, rightResult.Type, refAlignParam);
            var replaceArg = apply.ReplaceArg(rightResult);
            var result = replaceArg.ReplaceObjectRefByArg(refAlignParam, ObjectType());
            DumpWithBreak
                (
                    trace, "functionalFeature", functionalFeature, "apply", apply, "replaceArg",
                    replaceArg, "result", result);
            return ReturnMethodDump(trace, result);
        }

        internal Result ConvertToAsRef(Category category, AutomaticReferenceType target)
        {
            if(IsRefLike(target))
                return target.ArgResult(category);

            return LocalReferenceResult(category, target.RefAlignParam);
        }

        internal Result LocalReferenceResult(Category category, RefAlignParam refAlignParam)
        {
            if(this is AutomaticReferenceType)
            {
                return Align(refAlignParam.AlignBits)
                    .Result
                    (
                        category,
                        () =>
                        LocalReferenceCode(refAlignParam).Dereference
                            (refAlignParam, refAlignParam.RefSize),
                        () => Destructor(Category.Refs).Refs
                    );
            }
            return Align(refAlignParam.AlignBits)
                .SpawnReference(refAlignParam)
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

        internal Result ReplaceObjectReferenceByArg(Result result, RefAlignParam refAlignParam)
        {
            var objectReference = ObjectReference(refAlignParam);
            return result
                .ReplaceAbsolute
                (
                    objectReference
                    , () => LocalReferenceCode(refAlignParam)
                    , () => Destructor(Category.Refs).Refs
                );
        }

        internal virtual Result ReferenceInCode(IReferenceInCode target, Category category)
        {
            return SpawnReference(target.RefAlignParam)
                .Result
                (
                    category,
                    () => CodeBase.ReferenceCode(target),
                    () => Refs.Create(target)
                )
                ;
        }

        internal IContextItem SpawnFunction() { return _cache.Function.Value; }

        internal virtual Result ThisReferenceResult(Category category)
        {
            NotImplementedMethod(category);
            return null;
        }

        internal virtual AccessType AccessType(Structure accessPoint, int position)
        {
            return
                SpawnAccessType(accessPoint, position);
        }

        internal AccessType SpawnAccessType(Structure accessPoint, int position)
        {
            return _cache
                .AccessTypes
                .Find(accessPoint)
                .Find(position);
        }

        internal Result OperationResult<TFeature>(Category category, Defineable defineable, RefAlignParam refAlignParam)
            where TFeature : class
        {
            var trace = defineable.ObjectId == -25 && category.HasCode;
            StartMethodDumpWithBreak(trace, category, defineable, refAlignParam);
            var searchResult = SearchDefineable<TFeature>(defineable);
            var feature = searchResult.ConvertToFeature();
            if(feature == null)
                return ReturnMethodDump<Result>(trace, null);

            DumpWithBreak(trace, "feature", feature);
            var result = feature.Apply(category, refAlignParam);
            DumpWithBreak(trace, "result", result);
            var typeOfArgInApplyResult = feature.TypeOfArgInApplyResult(refAlignParam);
            if(ToReference(refAlignParam) != typeOfArgInApplyResult)
            {
                DumpWithBreak(trace, "typeOfArgInApplyResult", typeOfArgInApplyResult);
                var conversion = ToReference(refAlignParam).Conversion(category, typeOfArgInApplyResult);
                DumpWithBreak(trace, "conversion", conversion);
                result = result.ReplaceArg(conversion);
            }
            return ReturnMethodDumpWithBreak(trace, result);
        }

        private AutomaticReferenceType ObtainReference(RefAlignParam refAlignParam) { return new AutomaticReferenceType(this, refAlignParam); }

        protected virtual TypeBase ToReference(RefAlignParam refAlignParam) { return SpawnReference(refAlignParam); }
    }
}