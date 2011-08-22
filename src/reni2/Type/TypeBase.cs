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
using Reni.Feature.DumpPrint;
using Reni.Sequence;
using Reni.Struct;
using Reni.Syntax;
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
            public readonly SimpleCache<TypeType> TypeType;
            public readonly SimpleCache<TextItemType> TextItem;

            public Cache(TypeBase parent)
            {
                References = new DictionaryEx<RefAlignParam, AutomaticReferenceType>(parent.ObtainReference);
                Pairs = new DictionaryEx<TypeBase, Pair>(first => new Pair(first, parent));
                Sequences = new DictionaryEx<int, SequenceType>(elementCount => new SequenceType(parent, elementCount));
                Arrays = new DictionaryEx<int, Array>(count => new Array(parent, count));
                Aligners = new DictionaryEx<int, Aligner>(alignBits => new Aligner(parent, alignBits));
                TypeType = new SimpleCache<TypeType>(() => new TypeType(parent));
                TextItem = new SimpleCache<TextItemType>(() => new TextItemType(parent));
            }
        }

        private static int _nextObjectId;
        private readonly Cache _cache;
        [DisableDump]
        internal readonly ISearchPath<IPrefixFeature, ReferenceType> CreateArrayFromReferenceFeature;

        [UsedImplicitly]
        private static ReniObject _lastSearchVisitor;

        protected TypeBase()
            : base(_nextObjectId++)
        {
            CreateArrayFromReferenceFeature = new CreateArrayFromReferenceFeature(this);
            _cache = new Cache(this);
        }

        internal static Void Void { get { return Cache.Void; } }
        internal static TypeBase Bit { get { return Cache.Bit; } }

        [Node]
        internal Size Size { get { return GetSize(); } }

        protected abstract Size GetSize();

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

        internal virtual int SequenceCount(TypeBase elementType)
        {
            NotImplementedMethod(elementType);
            return 0;
        }

        [DisableDump]
        protected internal virtual int IndexSize { get { return 0; } }

        internal TypeBase UniqueAlign(int alignBits)
        {
            if(Size.Align(alignBits) == Size)
                return this;
            return _cache.Aligners.Find(alignBits);
        }

        internal Array UniqueArray(int count) { return _cache.Arrays.Find(count); }
        protected virtual TypeBase ReversePair(TypeBase first) { return first._cache.Pairs.Find(this); }
        internal virtual AutomaticReferenceType UniqueAutomaticReference(RefAlignParam refAlignParam) { return _cache.References.Find(refAlignParam); }
        internal SequenceType UniqueSequence(int elementCount) { return _cache.Sequences.Find(elementCount); }
        internal static TypeBase UniqueNumber(int bitCount) { return Bit.UniqueSequence(bitCount); }
        internal TextItemType UniqueTextItem() { return _cache.TextItem.Value; }
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
        internal Result ArgResult(Category category) { return Result(category, ArgCode, Refs.Arg); }
        internal Result Result(Result codeAndRefs) { return Result(codeAndRefs.CompleteCategory, codeAndRefs); }
        internal CodeBase ArgCode() { return CodeBase.Arg(this); }
        internal Result ReferenceArgResult(Category category, RefAlignParam refAlignParam) { return UniqueAutomaticReference(refAlignParam).ArgResult(category); }

        internal virtual Result AutomaticDereferenceResult(Category category) { return ArgResult(category); }

        internal Result Result(Category category)
        {
            return Result
                (
                    category,
                    () => CodeBase.BitsConst(Size, BitsConst.Convert(0).Resize(Size))
                    , Refs.Void);
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
            if(thenType.IsConvertable(elseType))
                return elseType;
            if(elseType.IsConvertable(thenType))
                return thenType;
            thenType.NotImplementedMethod(elseType);
            return null;
        }

        private Result ConvertToSequence(Category category, TypeBase elementType) { return Conversion(category, CreateSequenceType(elementType)); }

        internal Result ConvertToBitSequence(Category category) { return ConvertToSequence(category, Bit).Align(BitsConst.SegmentAlignBits); }

        /// <summary>
        ///     Gets the icon key.
        /// </summary>
        /// <value>The icon key.</value>
        string IIconKeyProvider.IconKey { get { return "Type"; } }

        [DisableDump]
        internal virtual RefAlignParam[] ReferenceChain { get { return new RefAlignParam[0]; } }

        [DisableDump]
        internal TypeType UniqueTypeType { get { return _cache.TypeType.Value; } }

        [DisableDump]
        internal virtual IFunctionalFeature FunctionalFeature { get { return this as IFunctionalFeature; } }

        [DisableDump]
        internal virtual IMetaFeature MetaFeature { get { return this as IMetaFeature; } }

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
        [DisableDump]
        internal virtual int ArrayElementCount { get { return 1; } }
        [DisableDump]
        internal virtual bool IsArray { get { return false; } }
        private TypeBase CreateSequenceType(TypeBase elementType) { return elementType.UniqueSequence(SequenceCount(elementType)); }

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

        internal virtual Result PropertyResult(Category category)
        {
            NotImplementedMethod();
            return null;
        }

        internal virtual Result LocalReferenceResult(Category category, RefAlignParam refAlignParam)
        {
            return UniqueAutomaticReference(refAlignParam)
                .Result
                (
                    category,
                    () => LocalReferenceCode(refAlignParam),
                    () => Destructor(Category.Refs).Refs + Refs.Arg()
                );
        }

        internal CodeBase LocalReferenceCode(RefAlignParam refAlignParam)
        {
            return ArgCode()
                .LocalReference(refAlignParam, Destructor(Category.Code).Code);
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

        internal Result OperationResult<TFeature>(Category category, Defineable defineable, RefAlignParam refAlignParam)
            where TFeature : class
        {
            var trace = ObjectId == -5 && defineable.ObjectId == 12 && (category.HasCode || category.HasType);
            StartMethodDump(trace, category, defineable, refAlignParam);
            try
            {
                BreakExecution();
                var searchResult = SearchDefineable<TFeature>(defineable);
                var feature = searchResult.ConvertToFeature();
                Dump("feature", feature);
                if(feature == null)
                    return ReturnMethodDump<Result>(null);
                BreakExecution();
                var featureResult = feature.ObtainResult(category.Refsd, refAlignParam);
                Dump("featureResult", featureResult);
                BreakExecution();
                var convertObject = ConvertObject(category.Typed, refAlignParam, feature);
                Dump("convertObject", convertObject);
                BreakExecution();
                var result = featureResult
                    .ReplaceArg(() => convertObject);
                return ReturnMethodDump(result, true);
            }
            finally
            {
                EndMethodDump();
            }
        }

        private Result ConvertObject(Category category, RefAlignParam refAlignParam, IFeature feature)
        {
            var trace = feature.GetObjectId() == -1 && category.HasCode;
            StartMethodDump(trace, category, refAlignParam, feature);
            try
            {
                var featureObject = feature.TypeOfArgInApplyResult(refAlignParam);
                Dump("featureObject", featureObject);
                BreakExecution();
                var reference = ForceReference(refAlignParam);
                if(reference == featureObject)
                    return ReturnMethodDump(featureObject.ArgResult(category), true);
                return ReturnMethodDump(reference.Conversion(category.Typed, featureObject), true);
            }
            finally
            {
                EndMethodDump();
            }
        }

        private AutomaticReferenceType ObtainReference(RefAlignParam refAlignParam) { return new AutomaticReferenceType(this, refAlignParam); }

        internal virtual TypeBase ForceReference(RefAlignParam refAlignParam) { return UniqueAutomaticReference(refAlignParam); }

        internal CodeBase BitSequenceOperation(ISequenceOfBitPrefixOperation token)
        {
            return UniqueAlign(BitsConst.SegmentAlignBits)
                .ArgCode()
                .BitSequenceOperation(token, Size);
        }

        internal Result GenericDumpPrintResult(Category category, RefAlignParam refAlignParam) { return OperationResult<IFeature>(category, DumpPrintToken.Create(), refAlignParam); }

        internal Result CreateArray(Category category, RefAlignParam refAlignParam)
        {
            return UniqueAlign(refAlignParam.AlignBits)
                .UniqueArray(1)
                .UniqueAutomaticReference(refAlignParam)
                .Result(category, ReferenceArgResult(category, refAlignParam));
        }

        internal Result Conversion(Category category, TypeBase destination)
        {
            if(category <= (Category.Size.Typed))
                return destination.Result(category);

            var result = Converter(destination).Result(category);
            if(category.HasType && result.Type != destination)
                DumpDataWithBreak("Wrong conversion result type", "this", this, "destination", destination, "result", result);
            return result;
        }

        private Converter Converter(TypeBase destination) { return Converter(ConversionParameter.Instance, destination); }

        private bool IsConvertable(TypeBase destination) { return Converter(destination).IsValid; }

        internal Converter Converter(ConversionParameter conversionParameter, TypeBase destination)
        {
            if(this == destination)
                return new FunctionalConverter(ArgResult);

            var referenceSource = this as ReferenceType;
            if(referenceSource != null)
                return referenceSource.Converter(conversionParameter, destination);

            var alignerSource = this as Aligner;
            if(alignerSource != null)
            {
                return
                    alignerSource.UnAlignedResult
                    *alignerSource.Parent.Converter(conversionParameter, destination);
            }

            var alignerDestination = destination as Aligner;
            if(alignerDestination != null)
            {
                return
                    Converter(conversionParameter, alignerDestination.Parent)
                    *alignerDestination.ParentToAlignedResult;
            }

            var sequenceSource = this as SequenceType;
            if(sequenceSource != null)
            {
                var sequenceDestination = destination as SequenceType;
                if(sequenceDestination != null)
                    return SequenceType.Converter(sequenceSource, conversionParameter, sequenceDestination);
                NotImplementedMethod(conversionParameter, destination);
                return null;
            }

            var enableCutSource = this as EnableCut;
            if(enableCutSource != null)
            {
                return
                    enableCutSource.StripTagResult
                    *enableCutSource.Parent.Converter(conversionParameter.EnableCut, destination);
            }

            if(IsZeroSized && destination is Void)
                return new FunctionalConverter(c => Void.Result(c, ArgResult(c)));

            var functionalSource = this as FunctionalBody.Type;
            if(functionalSource != null)
            {
                var arrayDestination = destination as Array;
                if(arrayDestination != null)
                    return new FunctionalConverter(category => arrayDestination.Result(category, functionalSource.FunctionalFeature));
                NotImplementedMethod(conversionParameter, destination);
                return null;
            }
            NotImplementedMethod(conversionParameter, destination);
            return null;
        }

        internal virtual Result DumpPrintTextResultFromSequence(Category category, RefAlignParam refAlignParam, int count)
        {
            NotImplementedMethod(category, refAlignParam, count);
            return null;
        }

        internal Result TextItemResult(Category category, RefAlignParam refAlignParam)
        {
            return
                UniqueTextItem()
                    .UniqueAutomaticReference(refAlignParam)
                    .Result(category, UniqueAutomaticReference(refAlignParam).ArgResult(category));
        }

        internal virtual Result UnAlignedResult(Category category) { return ArgResult(category); }
    }

    internal interface IMetaFeature
    {
        Result ObtainResult(Category category, ContextBase contextBase, CompileSyntax left, CompileSyntax right, RefAlignParam refAlignParam);
    }
}