#region Copyright (C) 2012

//     Project Reni2
//     Copyright (C) 2011 - 2012 Harald Hoyer
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

#endregion

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
using Reni.Feature;
using Reni.Feature.DumpPrint;
using Reni.Sequence;
using Reni.Struct;

namespace Reni.Type
{
    [Serializable]
    abstract class TypeBase : ReniObject, IDumpShortProvider, IIconKeyProvider
    {
        sealed class Cache
        {
            public static readonly Bit Bit = new Bit();
            public static readonly Void Void = new Void();
            public readonly DictionaryEx<int, Aligner> Aligners;
            public readonly DictionaryEx<int, Array> Arrays;
            public readonly DictionaryEx<int, SequenceType> Sequences;
            public readonly DictionaryEx<TypeBase, Pair> Pairs;
            public readonly DictionaryEx<RefAlignParam, IReference> References;
            public readonly SimpleCache<TypeType> TypeType;
            public readonly SimpleCache<TextItemType> TextItem;
            public readonly SimpleCache<EnableCut> EnableCut;

            public Cache(TypeBase parent)
            {
                EnableCut = new SimpleCache<EnableCut>(() => new EnableCut(parent));
                References = new DictionaryEx<RefAlignParam, IReference>(parent.ObtainReference);
                Pairs = new DictionaryEx<TypeBase, Pair>(first => new Pair(first, parent));
                Sequences = new DictionaryEx<int, SequenceType>(elementCount => new SequenceType(parent, elementCount));
                Arrays = new DictionaryEx<int, Array>(count => new Array(parent, count));
                Aligners = new DictionaryEx<int, Aligner>(alignBits => new Aligner(parent, alignBits));
                TypeType = new SimpleCache<TypeType>(() => new TypeType(parent));
                TextItem = new SimpleCache<TextItemType>(() => new TextItemType(parent));
            }
        }

        static int _nextObjectId;
        readonly Cache _cache;
        [DisableDump]
        internal readonly ISearchPath<IPrefixFeature, AutomaticReferenceType> CreateArrayFromReferenceFeature;

        [UsedImplicitly]
        static ReniObject _lastSearchVisitor;

        protected TypeBase()
            : base(_nextObjectId++)
        {
            CreateArrayFromReferenceFeature = new CreateArrayFromReferenceFeature(this);
            _cache = new Cache(this);
        }

        internal static Void Void { get { return Cache.Void; } }
        internal static TypeBase Bit { get { return Cache.Bit; } }

        [Node]
        internal Size Size
        {
            get
            {
                if(IsDataLess)
                    return Size.Zero;
                return GetSize();
            }
        }

        [NotNull]
        protected virtual Size GetSize()
        {
            NotImplementedMethod();
            return Size.Zero;
        }

        [DisableDump]
        internal virtual bool IsDataLess
        {
            get
            {
                NotImplementedMethod();
                return true;
            }
        }

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

        [DisableDump]
        internal TextItemType UniqueTextItemType { get { return _cache.TextItem.Value; } }
        [DisableDump]
        internal EnableCut UniqueEnableCutType { get { return _cache.EnableCut.Value; } }
        [DisableDump]
        internal virtual TypeBase AutomaticDereferenceType { get { return this; } }
        [DisableDump]
        internal virtual TypeBase TypeForTypeOperator { get { return this; } }

        internal Array UniqueArray(int count) { return _cache.Arrays.Find(count); }
        protected virtual TypeBase ReversePair(TypeBase first) { return first._cache.Pairs.Find(this); }
        internal IReference UniqueReference(RefAlignParam refAlignParam) { return _cache.References.Find(refAlignParam); }
        internal SequenceType UniqueSequence(int elementCount) { return _cache.Sequences.Find(elementCount); }
        internal static TypeBase UniqueNumber(int bitCount) { return Bit.UniqueSequence(bitCount); }
        internal virtual TypeBase Pair(TypeBase second) { return second.ReversePair(this); }
        static Result VoidCodeAndRefs(Category category) { return VoidResult(category & (Category.Code | Category.CodeArgs)); }
        internal static Result VoidResult(Category category) { return Void.Result(category); }
        internal virtual Result Destructor(Category category) { return VoidCodeAndRefs(category); }
        internal virtual Result ArrayDestructor(Category category, int count) { return VoidCodeAndRefs(category); }
        internal virtual Result Copier(Category category) { return VoidCodeAndRefs(category); }
        internal virtual Result ArrayCopier(Category category, int count) { return VoidCodeAndRefs(category); }
        internal virtual Result ApplyTypeOperator(Result argResult) { return argResult.Type.Conversion(argResult.CompleteCategory, this).ReplaceArg(argResult); }
        internal Result ArgResult(Category category) { return Result(category, ArgCode, CodeArgs.Arg); }
        internal CodeBase ArgCode() { return CodeBase.Arg(this); }
        internal Result ReferenceArgResult(Category category, RefAlignParam refAlignParam) { return UniqueReference(refAlignParam).Type.ArgResult(category); }
        internal CodeBase DereferencedReferenceCode(RefAlignParam refAlignParam) { return UniqueReference(refAlignParam).Type.ArgCode().Dereference(Size); }

        internal Result Result(Category category)
        {
            return Result
                (
                    category,
                    () => CodeBase.BitsConst(Size, BitsConst.Convert(0).Resize(Size))
                    , CodeArgs.Void);
        }

        internal Result Result(Category category, IReferenceInCode target)
        {
            if(IsDataLess)
                return Result(category);
            return new Result
                (category
                 , () => false
                 , () => target.RefAlignParam.RefSize
                 , () => this
                 , () => CodeBase.ReferenceCode(target)
                 , () => CodeArgs.Create(target));
        }

        internal Result Result(Category category, Result codeAndRefs)
        {
            var result = new Result();
            if(category.HasCode)
                result.Code = codeAndRefs.Code;
            if(category.HasArgs)
                result.CodeArgs = codeAndRefs.CodeArgs;
            if(category.HasType)
                result.Type = this;
            result.Amend(category, this);
            return result;
        }

        internal Result Result(Category category, Func<CodeBase> getCode, Func<CodeArgs> getRefs)
        {
            var result = new Result();
            if(category.HasCode)
                result.Code = getCode();
            if(category.HasArgs)
                result.CodeArgs = getRefs();
            if(category.HasType)
                result.Type = this;
            result.Amend(category, this);
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

        Result ConvertToSequence(Category category, TypeBase elementType) { return Conversion(category, CreateSequenceType(elementType)); }

        internal Result ConvertToBitSequence(Category category) { return ConvertToSequence(category, Bit).Align(BitsConst.SegmentAlignBits); }

        /// <summary>
        ///     Gets the icon key.
        /// </summary>
        /// <value> The icon key. </value>
        string IIconKeyProvider.IconKey { get { return "Type"; } }

        [DisableDump]
        internal virtual RefAlignParam[] ReferenceChain { get { return new RefAlignParam[0]; } }

        [DisableDump]
        internal TypeType UniqueTypeType { get { return _cache.TypeType.Value; } }

        [DisableDump]
        internal virtual IFunctionalFeature FunctionalFeature { get { return this as IFunctionalFeature; } }
        [DisableDump]
        internal IFunctionalFeatureSpecial FunctionalFeatureSpecial { get { return this as IFunctionalFeatureSpecial; } }

        [DisableDump]
        internal IMetaFeature MetaFeature { get { return this as IMetaFeature; } }

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
        internal virtual TypeBase UnAlignedType { get { return this; } }
        [DisableDump]
        internal virtual int ArrayElementCount { get { return 1; } }
        [DisableDump]
        internal virtual bool IsArray { get { return false; } }
        [DisableDump]
        internal virtual bool IsLambda { get { return false; } }
        [DisableDump]
        internal virtual bool IsLikeReference { get { return false; } }
        [DisableDump]
        internal virtual IReference Reference { get { return this as IReference; } }

        TypeBase CreateSequenceType(TypeBase elementType) { return elementType.UniqueSequence(SequenceCount(elementType)); }

        internal SearchResult Search<TFeature>(ISearchTarget target)
            where TFeature : class, ITypeFeature
        {
            var visitor = new TypeRootSearchVisitor<TFeature>(target, this);
            visitor.Search(this);
            if(Debugger.IsAttached)
                _lastSearchVisitor = visitor;
            return visitor.SearchResult;
        }

        internal abstract void Search(SearchVisitor searchVisitor);

        internal virtual Result SmartLocalReferenceResult(Category category, RefAlignParam refAlignParam)
        {
            if(IsDataLess)
                return ArgResult(category);

            return UniqueReference(refAlignParam)
                .Type
                .Result
                (
                    category,
                    () => LocalReferenceCode(refAlignParam),
                    () => Destructor(Category.CodeArgs).CodeArgs + CodeArgs.Arg()
                );
        }

        internal CodeBase LocalReferenceCode(RefAlignParam refAlignParam)
        {
            return ArgCode()
                .LocalReference(refAlignParam, Destructor(Category.Code).Code);
        }

        internal Result ReferenceInCode(Category category, IReferenceInCode target)
        {
            return UniqueReference(target.RefAlignParam)
                .Type
                .Result(category, target);
        }

        internal Result OperationResult<TFeature>(Category category, ISearchTarget target, RefAlignParam refAlignParam)
            where TFeature : class, ITypeFeature
        {
            var trace = ObjectId == 10 && target.GetObjectId() == 34 && category.HasCode;
            StartMethodDump(trace, category, target, refAlignParam);
            try
            {
                BreakExecution();
                var feature = Search<TFeature>(target);
                Dump("feature", feature);
                if(feature == null)
                    return ReturnMethodDump<Result>(null);
                BreakExecution();
                var result = feature.Result(category, refAlignParam);
                return ReturnMethodDump(result, true);
            }
            finally
            {
                EndMethodDump();
            }
        }

        IReference ObtainReference(RefAlignParam refAlignParam)
        {
            var result = Reference;
            if(result == null)
                result = new AutomaticReferenceType(this, refAlignParam);
            return result;
        }

        internal TypeBase SmartReference(RefAlignParam refAlignParam)
        {
            if(IsDataLess)
                return this;
            return UniqueReference(refAlignParam).Type;
        }

        internal CodeBase BitSequenceOperation(ISequenceOfBitPrefixOperation token)
        {
            return UniqueAlign(BitsConst.SegmentAlignBits)
                .ArgCode()
                .BitSequenceOperation(token, Size);
        }

        internal Result GenericDumpPrintResult(Category category, RefAlignParam refAlignParam) { return OperationResult<ISuffixFeature>(category, _dumpPrintToken.Value, refAlignParam); }
        private static readonly SimpleCache<DumpPrintToken> _dumpPrintToken = new SimpleCache<DumpPrintToken>(DumpPrintToken.Create); 

        internal Result CreateArray(Category category, RefAlignParam refAlignParam)
        {
            return UniqueAlign(refAlignParam.AlignBits)
                .UniqueArray(1)
                .UniqueReference(refAlignParam)
                .Type
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

        IConverter Converter(TypeBase destination) { return Converter(ConversionParameter.Instance, destination); }

        bool IsConvertable(TypeBase destination) { return Converter(destination) != null; }

        internal IConverter Converter(ConversionParameter conversionParameter, TypeBase destination)
        {
            if(this == destination)
                return new FunctionalConverter(ArgResult);

            return ConverterForDifferentTypes(conversionParameter, destination);
        }

        protected virtual IConverter ConverterForDifferentTypes(ConversionParameter conversionParameter, TypeBase destination)
        {
            var alignerDestination = destination as Aligner;
            if(alignerDestination != null)
                return Converter(conversionParameter, alignerDestination);

            return ConverterForUnalignedTypes(conversionParameter, destination);
        }

        protected virtual IConverter ConverterForUnalignedTypes(ConversionParameter conversionParameter, TypeBase destination)
        {
            if(IsDataLess && destination is Void)
                return new FunctionalConverter(c => Void.Result(c, ArgResult(c)));

            NotImplementedMethod(conversionParameter, destination);
            return null;
        }

        IConverter Converter(ConversionParameter conversionParameter, Aligner alignerDestination)
        {
            return
                Converter(conversionParameter, alignerDestination.Parent)
                    .Concat(alignerDestination.ParentToAlignedResult);
        }

        internal virtual Result DumpPrintTextResultFromSequence(Category category, RefAlignParam refAlignParam, int count)
        {
            NotImplementedMethod(category, refAlignParam, count);
            return null;
        }

        internal Result TextItemResult(Category category, RefAlignParam refAlignParam)
        {
            var uniqueTextItem = UniqueTextItemType;
            return
                uniqueTextItem
                    .UniqueReference(refAlignParam)
                    .Type
                    .Result(category, UniqueReference(refAlignParam).Type.ArgResult(category))
                ;
        }

        internal virtual bool? IsDataLessStructureElement(bool isQuick) { return Size.IsZero; }

        protected Result AssignmentResult(Category category, TypeBase argsType, ISetterTargetType target)
        {
            var trace = ObjectId == 9 && category.HasCode;
            StartMethodDump(trace, category, argsType, target);
            try
            {
                var sourceResult = argsType.Conversion(category, target.ValueType.UniqueReference(target.RefAlignParam).Type);
                var destinationResult = target
                    .DestinationResult(category.Typed)
                    .ReplaceArg(target.Type.Result(category.Typed, target));
                ;
                var resultForArg = destinationResult + sourceResult;
                Dump("resultForArg", resultForArg);

                BreakExecution();

                var rawResult = target.Result(category);
                Dump("rawResult", rawResult);

                BreakExecution();

                var result = rawResult.ReplaceArg(resultForArg);
                return ReturnMethodDump(result, true);
            }
            finally
            {
                EndMethodDump();
            }
        }
    }
}