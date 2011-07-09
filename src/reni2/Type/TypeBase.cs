//     Compiler for programming language "Reni"
//     Copyright (C) 2011 Harald Hoyer
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//     
//     Comments, bugs and suggestions to hahoyer at yahoo.de

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
            public readonly DictionaryEx<int, SequenceType> Sequences;
            public readonly DictionaryEx<TypeBase, Pair> Pairs;
            public readonly DictionaryEx<RefAlignParam, AutomaticReferenceType> References;
            public readonly DictionaryEx<RefAlignParam, ObjectReference> ObjectReferences;
            public readonly SimpleCache<TypeType> TypeType;
            public readonly SimpleCache<Context.Function> Function;
            public readonly DictionaryEx<RefAlignParam, DictionaryEx<IFunctionalFeature, TypeBase>> FunctionalTypes;
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
                Sequences = new DictionaryEx<int, SequenceType>(elementCount => new SequenceType(parent, elementCount));
                Arrays = new DictionaryEx<int, Array>(count => new Array(parent, count));
                Aligners = new DictionaryEx<int, Aligner>(alignBits => new Aligner(parent, alignBits));
                TypeType = new SimpleCache<TypeType>(() => new TypeType(parent));
                Function = new SimpleCache<Context.Function>(() => new Context.Function(parent));
                FunctionalTypes = new DictionaryEx<RefAlignParam, DictionaryEx<IFunctionalFeature, TypeBase>>(
                    refAlignParam => new DictionaryEx<IFunctionalFeature, TypeBase>(
                                         feature => new FunctionalFeatureType<IFunctionalFeature>(parent, feature, refAlignParam)
                                         )
                    );
            }
        }

        private static int _nextObjectId;
        private readonly Cache _cache;

        [UsedImplicitly]
        private static ReniObject _lastSearchVisitor;


        protected TypeBase()
            : base(_nextObjectId++) { _cache = new Cache(this); }

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

        internal Array UniqueArray(int count) { return _cache.Arrays.Find(count); }
        protected virtual TypeBase ReversePair(TypeBase first) { return first._cache.Pairs.Find(this); }
        internal virtual AutomaticReferenceType UniqueAutomaticReference(RefAlignParam refAlignParam) { return _cache.References.Find(refAlignParam); }
        internal SequenceType UniqueSequence(int elementCount) { return _cache.Sequences.Find(elementCount); }
        internal ObjectReference UniqueObjectReference(RefAlignParam refAlignParam) { return _cache.ObjectReferences.Find(refAlignParam); }
        internal static TypeBase Number(int bitCount) { return Bit.UniqueSequence(bitCount); }
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
        internal Result ArgResult(Category category) { return Result(category, ArgCode); }
        internal Result Result(Result codeAndRefs) { return Result(codeAndRefs.CompleteCategory, codeAndRefs); }
        internal Result Result(Category category, Func<CodeBase> getCode) { return Result(category, getCode, Refs.None); }
        internal CodeBase ArgCode() { return CodeBase.Arg(this); }

        internal virtual Result AutomaticDereferenceResult(Category category) { return ArgResult(category); }

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
            if(thenType.IsConvertable(elseType, ConversionParameter.Instance))
                return elseType;
            if(elseType.IsConvertable(thenType, ConversionParameter.Instance))
                return thenType;
            thenType.NotImplementedMethod(elseType);
            return null;
        }

        internal Result Conversion(Category category, TypeBase dest)
        {
            if(category <= (Category.Size | Category.Type))
                return dest.Result(category);
            if(IsConvertable(dest, ConversionParameter.Instance))
                return ForceConversion(category, dest);
            NotImplementedMethod(category, dest);
            return null;
        }

        internal Result ForceConversion(Category category, TypeBase destination)
        {
            if(this == destination)
                return InternalConversionToItself(category);
            return destination.VirtualForceConversionFrom(category, this);
        }

        internal bool IsConvertable(TypeBase destination, ConversionParameter conversionParameter)
        {
            if(this == destination)
                return IsConvertableToItself(conversionParameter);
            return destination.VirtualIsConvertableFrom(this, conversionParameter);
        }

        private Result ConvertToSequence(Category category, TypeBase elementType) { return Conversion(category, CreateSequenceType(elementType)); }

        internal Result ConvertToBitSequence(Category category) { return ConvertToSequence(category, Bit).Align(BitsConst.SegmentAlignBits); }

        protected virtual Result InternalConversionToItself(Category category) { return ArgResult(category); }

        protected virtual Result VirtualForceConversionFrom(Category category, TypeBase source)
        {
            NotImplementedMethod(category, source);
            return null;
        }

        protected virtual bool VirtualIsConvertableFrom(TypeBase source, ConversionParameter conversionParameter)
        {
            NotImplementedMethod(source, conversionParameter);
            return false;
        }

        internal virtual bool HasConverterTo(TypeBase destination) { return false; }

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

        [DisableDump]
        internal virtual IFunctionalFeature FunctionalFeature
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }

        [DisableDump]
        internal virtual TypeBase ObjectType
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }

        [DisableDump]
        internal virtual Structure FindRecentStructure
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }

        [DisableDump]
        internal virtual bool IsZeroSized { get { return Size.IsZero; } }

        [DisableDump]
        internal virtual TypeBase UnAlignedType { get { return this; } }

        protected bool IsRefLike(AutomaticReferenceType target) { return false; }

        private TypeBase CreateSequenceType(TypeBase elementType) { return elementType.UniqueSequence(SequenceCount(elementType)); }

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

        internal virtual Result PropertyResult(Category category)
        {
            NotImplementedMethod();
            return null;
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
            return UniqueAutomaticReference(refAlignParam)
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
            var objectReference = UniqueObjectReference(refAlignParam);
            return result
                .ReplaceAbsolute
                (
                    objectReference
                    , () => UniqueAutomaticReference(refAlignParam).ArgCode()
                    , Refs.None
                );
        }

        internal virtual Result ReferenceInCode(IReferenceInCode target, Category category)
        {
            return UniqueAutomaticReference(target.RefAlignParam)
                .Result
                (
                    category,
                    () => CodeBase.ReferenceCode(target),
                    () => Refs.Create(target)
                )
                ;
        }

        internal IContextItem UniqueFunction() { return _cache.Function.Value; }

        internal virtual AccessType AccessType(Structure accessPoint, int position)
        {
            return
                UniqueAccessType(accessPoint, position);
        }

        internal AccessType UniqueAccessType(Structure accessPoint, int position)
        {
            return _cache
                .AccessTypes
                .Find(accessPoint)
                .Find(position);
        }

        internal Result OperationResult<TFeature>(Category category, Defineable defineable, RefAlignParam refAlignParam)
            where TFeature : class
        {
            var trace = defineable.ObjectId == 20 && category.HasCode;
            StartMethodDump(trace, category, defineable, refAlignParam);
            try
            {
                BreakExecution();
                var searchResult = SearchDefineable<TFeature>(defineable);
                var feature = searchResult.ConvertToFeature();
                if(feature == null)
                    return ReturnMethodDump<Result>(null);

                Dump("feature", feature);
                var result = feature.Apply(category, refAlignParam);
                Dump("result", result);
                if(result.HasArg)
                {
                    var typeOfArgInApplyResult = feature.TypeOfArgInApplyResult(refAlignParam);
                    var reference = ToReference(refAlignParam);
                    Dump("reference", reference);
                    if (reference != typeOfArgInApplyResult)
                    {
                        Dump("typeOfArgInApplyResult", typeOfArgInApplyResult);
                        BreakExecution();
                        var conversion = reference.Conversion(category.Typed, typeOfArgInApplyResult);
                        Dump("conversion", conversion);
                        result = result.ReplaceArg(conversion);
                    }
                }
                return ReturnMethodDump(result,true);
            }
            finally
            {
                EndMethodDump();
            }
        }

        private AutomaticReferenceType ObtainReference(RefAlignParam refAlignParam) { return new AutomaticReferenceType(this, refAlignParam); }

        internal virtual TypeBase ToReference(RefAlignParam refAlignParam) { return UniqueAutomaticReference(refAlignParam); }

        internal TypeBase UniqueFunctionalType(IFunctionalFeature functionalFeature, RefAlignParam refAlignParam) { return _cache.FunctionalTypes.Find(refAlignParam).Find(functionalFeature); }

        internal virtual bool VirtualIsConvertable(SequenceType destination, ConversionParameter conversionParameter)
        {
            NotImplementedMethod(destination, conversionParameter);
            return false;
        }

        internal virtual bool VirtualIsConvertable(AutomaticReferenceType destination, ConversionParameter conversionParameter)
        {
            NotImplementedMethod(destination, conversionParameter);
            return false;
        }

        internal virtual bool VirtualIsConvertable(Aligner destination, ConversionParameter conversionParameter)
        {
            NotImplementedMethod(destination, conversionParameter);
            return false;
        }

        internal virtual Result VirtualForceConversion(Category category, SequenceType destination)
        {
            NotImplementedMethod(category, destination);
            return null;
        }

        internal virtual Result VirtualForceConversion(Category category, AutomaticReferenceType destination)
        {
            NotImplementedMethod(category, destination);
            return null;
        }

        internal virtual bool VirtualIsConvertable(Bit destination, ConversionParameter conversionParameter)
        {
            NotImplementedMethod(destination, conversionParameter);
            return false;
        }

        internal virtual Result VirtualForceConversion(Category category, Bit destination)
        {
            NotImplementedMethod(category, destination);
            return null;
        }
    }
}