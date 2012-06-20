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
using Reni.Context;
using Reni.Feature;
using Reni.Feature.DumpPrint;
using Reni.Struct;

namespace Reni.Type
{
    [Serializable]
    abstract class TypeBase
        : ReniObject
        , IContextReference
          , IDumpShortProvider
          , IIconKeyProvider
          , ISearchTarget
          , ISearchPath<ISuffixFeature, Aligner>
          , ISearchPath<ISuffixFeature, TypeBase>
    {
        sealed class Cache
        {
            public static readonly Bit Bit = new Bit();
            public static readonly Void Void = new Void();
            public readonly DictionaryEx<int, Aligner> Aligners;
            public readonly DictionaryEx<int, Array> Arrays;
            public readonly DictionaryEx<TypeBase, Pair> Pairs;
            public readonly SimpleCache<IReference> References;
            public readonly SimpleCache<TypeType> TypeType;
            public readonly SimpleCache<FunctionInstanceType> FunctionInstanceType;
            public readonly SimpleCache<TextItemType> TextItem;
            public readonly SimpleCache<EnableCut> EnableCut;

            public Cache(TypeBase parent)
            {
                EnableCut = new SimpleCache<EnableCut>(() => new EnableCut(parent));
                References = new SimpleCache<IReference>(parent.ObtainReference);
                Pairs = new DictionaryEx<TypeBase, Pair>(first => new Pair(first, parent));
                Arrays = new DictionaryEx<int, Array>(parent.ObtainArray);
                Aligners = new DictionaryEx<int, Aligner>(alignBits => new Aligner(parent, alignBits));
                FunctionInstanceType = new SimpleCache<FunctionInstanceType>(() => new FunctionInstanceType(parent));
                TypeType = new SimpleCache<TypeType>(() => new TypeType(parent));
                TextItem = new SimpleCache<TextItemType>(() => new TextItemType(parent));
            }
        }

        static int _nextObjectId;
        readonly Cache _cache;

        [UsedImplicitly]
        static ReniObject _lastSearchVisitor;

        protected TypeBase()
            : base(_nextObjectId++) { _cache = new Cache(this); }

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

        Size IContextReference.Size { get { return Size; } }
        [DisableDump]
        internal virtual Size UnrefSize { get { return Size; } }

        [DisableDump]
        protected internal virtual TypeBase[] ToList { get { return new[] {this}; } }

        string IDumpShortProvider.DumpShort() { return DumpShort(); }

        string ISearchTarget.StructFeatureName { get { return null; } }

        [DisableDump]
        internal virtual string DumpPrintText
        {
            get
            {
                NotImplementedMethod();
                return "";
            }
        }

        internal virtual int? SmartSequenceLength(TypeBase elementType) { return SmartArrayLength(elementType); }

        internal virtual int? SmartArrayLength(TypeBase elementType)
        {
            if (IsConvertable(elementType))
                return 1;
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
        internal virtual IReference UniqueReference { get { return _cache.References.Value; } }
        [DisableDump]
        internal virtual TypeBase TypeForTypeOperator { get { return this; } }
        [DisableDump]
        internal CodeBase ArgCode { get { return CodeBase.Arg(this); } }

        [DisableDump]
        internal TypeBase AutomaticDereferenceType
        {
            get
            {
                var result = Reference;
                if(result == null)
                    return this;
                return result.TargetType.AutomaticDereferenceType;
            }
        }

        internal Array UniqueArray(int count) { return _cache.Arrays.Find(count); }
        protected virtual TypeBase ReversePair(TypeBase first) { return first._cache.Pairs.Find(this); }
        internal static TypeBase UniqueNumber(int bitCount) { return Bit.UniqueArray(bitCount).UniqueSequence; }
        internal virtual TypeBase Pair(TypeBase second) { return second.ReversePair(this); }
        static Result VoidCodeAndRefs(Category category) { return VoidResult(category & (Category.Code | Category.CodeArgs)); }
        internal static Result VoidResult(Category category) { return Void.Result(category); }
        internal virtual Result Destructor(Category category) { return VoidCodeAndRefs(category); }
        internal virtual Result ArrayDestructor(Category category, int count) { return VoidCodeAndRefs(category); }
        internal virtual Result Copier(Category category) { return VoidCodeAndRefs(category); }
        internal virtual Result ArrayCopier(Category category, int count) { return VoidCodeAndRefs(category); }
        internal virtual Result ApplyTypeOperator(Result argResult) { return argResult.Type.Conversion(argResult.CompleteCategory, this).ReplaceArg(argResult); }
        internal Result ArgResult(Category category) { return Result(category, () => ArgCode, CodeArgs.Arg); }
        internal Result ReferenceArgResult(Category category) { return UniqueReference.Type().ArgResult(category); }

        internal Result Result(Category category, IContextReference target)
        {
            if(IsDataLess)
                return Result(category);
            return new Result
                (category
                 , getType: () => this
                 , getCode: () => CodeBase.ReferenceCode(target)
                );
        }

        internal Result Result(Category category, Result codeAndRefs)
        {
            return new Result
                (category
                 , getType: () => this
                 , getCode: () => codeAndRefs.Code
                 , getArgs: () => codeAndRefs.CodeArgs
                );
        }

        internal Result Result(Category category, Func<Category, Result> getCodeAndRefs)
        {
            var localCategory = category & (Category.Code | Category.CodeArgs);
            var codeAndRefs = getCodeAndRefs(localCategory);
            return Result
                (category
                 , () => codeAndRefs.Code
                 , () => codeAndRefs.CodeArgs
                );
        }

        internal Result Result(Category category, Func<CodeBase> getCode = null, Func<CodeArgs> getArgs = null)
        {
            return new Result
                (category
                 , getType: () => this
                 , getCode: getCode
                 , getArgs: getArgs
                );
        }

        internal TypeBase CommonType(TypeBase elseType) { return elseType.IsConvertable(this) ? this : elseType; }

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
        internal TypeBase UniqueFunctionInstanceType { get { return _cache.FunctionInstanceType.Value; } }

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
        internal virtual bool IsLambda { get { return false; } }
        [DisableDump]
        internal virtual IReference Reference { get { return this as IReference; } }
        [DisableDump]
        internal virtual IFeature Feature { get { return null; } }
        [DisableDump]
        protected virtual ISearchTarget ConversionProvider { get { return this; } }

        TypeBase CreateSequenceType(TypeBase elementType)
        {
            return elementType
                .UniqueArray(SequenceLength(elementType))
                .UniqueSequence;
        }

        internal int SequenceLength(TypeBase elementType)
        {
            var length = SmartSequenceLength(elementType);
            if(length != null)
                return length.Value;
            
            NotImplementedMethod(elementType);
            return -1;
        }

        internal int ArrayLength(TypeBase elementType)
        {
            var length = SmartArrayLength(elementType);
            if (length != null)
                return length.Value;

            NotImplementedMethod(elementType);
            return -1;
        }

        internal SearchResult Search<TFeature>(ISearchTarget target)
            where TFeature : class, IFeature
        {
            var visitor = new TypeRootSearchVisitor<TFeature>(target, this);
            Search(visitor);
            if(Debugger.IsAttached && !visitor.IsSuccessFull)
                _lastSearchVisitor = visitor;
            return visitor.SearchResult;
        }

        internal virtual void Search(SearchVisitor searchVisitor) { searchVisitor.Search(this, null); }

        internal virtual Result SmartLocalReferenceResult(Category category)
        {
            if(IsDataLess)
                return ArgResult(category);

            return UniqueReference.Type()
                .Result
                (
                    category,
                    LocalReferenceCode,
                    () => Destructor(Category.CodeArgs).CodeArgs + CodeArgs.Arg()
                );
        }

        internal CodeBase LocalReferenceCode()
        {
            return ArgCode
                .LocalReference(Destructor(Category.Code).Code);
        }

        internal Result ReferenceInCode(Category category, IContextReference target)
        {
            return UniqueReference.Type()
                .Result(category, target);
        }

        internal Result ContextAccessResult(Category category, IContextReference target, Size offset)
        {
            if(IsDataLess)
                return Result(category);
            return new Result
                (category
                 , getType: () => this
                 , getCode: () => CodeBase.ReferenceCode(target).AddToReference(offset).Dereference(Size)
                );
        }

        internal Result OperationResult<TFeature>(Category category, ISearchTarget target)
            where TFeature : class, IFeature
        {
            var trace = ObjectId == -15 && target.GetObjectId() == 40 && category.HasCode;
            StartMethodDump(trace, category, target);
            try
            {
                BreakExecution();
                var feature = Search<TFeature>(target);
                Dump("feature", feature);
                if(feature == null)
                    return ReturnMethodDump<Result>(null);
                BreakExecution();
                var result = feature.Result(category);
                return ReturnMethodDump(result);
            }
            finally
            {
                EndMethodDump();
            }
        }

        IReference ObtainReference() { return this as IReference ?? new AutomaticReferenceType(this); }

        protected virtual Array ObtainArray(int count) { return new Array(this, count); }

        internal TypeBase SmartReference()
        {
            if(IsDataLess)
                return this;
            return UniqueReference.Type();
        }

        internal CodeBase BitSequenceOperation(ISequenceOfBitPrefixOperation token)
        {
            return UniqueAlign(BitsConst.SegmentAlignBits).ArgCode
                .BitSequenceOperation(token, Size);
        }

        internal Result GenericDumpPrintResult(Category category) { return OperationResult<ISuffixFeature>(category, _dumpPrintToken.Value); }
        static readonly SimpleCache<DumpPrintToken> _dumpPrintToken = new SimpleCache<DumpPrintToken>(DumpPrintToken.Create);

        internal Result CreateArray(Category category)
        {
            return UniqueAlign(Root.DefaultRefAlignParam.AlignBits)
                .UniqueArray(1).UniqueReference.Type()
                .Result(category, ReferenceArgResult(category));
        }

        internal Result Conversion(Category category, TypeBase destination)
        {
            if(category <= (Category.Type.Replenished))
                return destination.Result(category);

            var smartConversionResult = ArgResult(category.Typed).SmartConversionResult(destination);
            if(smartConversionResult != null)
                return smartConversionResult;

            var searchResult = SmartConverter(destination);
            if(searchResult == null)
            {
                NotImplementedMethod(category, destination);
                return null;
            }

            var rawResult = searchResult.Result(category.Typed);
            var result = rawResult.ReplaceArg(ArgResult);
            return result.PostConversionResult(destination) & category;
        }
        SearchResult SmartConverter(TypeBase destination)
        {
            var searchResult = Converter(destination);
            if(searchResult != null)
                return searchResult;

            if(Reference != null)
                searchResult = Reference.TargetType.Converter(destination);
            if(searchResult != null)
                return searchResult;

            if(destination.Reference != null)
                searchResult = Converter(destination.Reference.TargetType);
            return searchResult;
        }

        internal SearchResult Converter(TypeBase destination)
        {
            return
                Search<ISuffixFeature>(destination.ConversionProvider);
        }

        internal bool IsConvertable(TypeBase destination) { return SmartConverter(destination) != null; }

        internal virtual Result DumpPrintTextResultFromSequence(Category category, int count)
        {
            NotImplementedMethod(category, count);
            return null;
        }

        internal Result TextItemResult(Category category)
        {
            var uniqueTextItem = UniqueTextItemType;
            return
                uniqueTextItem.UniqueReference.Type()
                    .Result(category, UniqueReference.Type().ArgResult(category))
                ;
        }

        internal virtual Result ConstructorResult(Category category, TypeBase argsType)
        {
            NotImplementedMethod(category, argsType);
            return null;
        }

        internal Result ConcatArrayFromReference(Category category, AutomaticReferenceType automaticReferenceType)
        {
            NotImplementedMethod(category, automaticReferenceType);
            return null;
        }

        Result DereferenceReferenceResult(Category category)
        {
            return UniqueReference
                .Type()
                .ArgResult(category.Typed)
                .DereferenceResult();
        }

        internal Result UnalignedDereferenceReferenceResult(Category category) { return DereferenceReferenceResult(category).SmartUn<Aligner>(); }

        internal Result BitSequenceOperandConversion(Category category)
        {
            return ConvertToBitSequence(category)
                .AutomaticDereferenceResult()
                .Align(BitsConst.SegmentAlignBits);
        }

        internal virtual ISuffixFeature AlignConversion(TypeBase destination)
        {
            var childConverter = Converter(destination);
            if(childConverter != null)
                return new AlignConverter(childConverter);

            return null;
        }

        internal Result DumpPrintTypeNameResult(Category category)
        {
            return Void
                .Result
                (category
                 , () => CodeBase.DumpPrintText(DumpPrintText)
                 , CodeArgs.Void
                );
        }

        ISuffixFeature ISearchPath<ISuffixFeature, Aligner>.Convert(Aligner type) { return type.UnAlignConversion(this); }
        ISuffixFeature ISearchPath<ISuffixFeature, TypeBase>.Convert(TypeBase type) { return Convert(type); }

        protected virtual ISuffixFeature Convert(TypeBase type)
        {
            if(type == this)
                return Extension.Feature(DereferenceReferenceResult);

            return null;
        }

        internal TypeBase SmartUn<T>()
            where T : IConverter { return this is T ? ((IConverter) this).Result(Category.Type).Type : this; }
        
        internal Result ReferenceConversionResult(Category category, TypeBase destinationType) { return destinationType.UniqueReference.Type().Result(category, UniqueReference.Type().ArgResult(category.Typed)); }
    }

    abstract class ConverterBase : ReniObject, ISuffixFeature, ISimpleFeature
    {
        IMetaFunctionFeature IFeature.MetaFunction { get { return null; } }
        IFunctionFeature IFeature.Function { get { return null; } }
        ISimpleFeature IFeature.Simple { get { return this; } }
        Result ISimpleFeature.Result(Category category) { return Result(category); }
        protected abstract Result Result(Category category);
    }

    sealed class AlignConverter : ConverterBase
    {
        [EnableDump]
        readonly SearchResult _childConverter;
        public AlignConverter(SearchResult childConverter) { _childConverter = childConverter; }
        protected override Result Result(Category category) { return _childConverter.Result(category.Typed); }
    }

    // Krautpuster
    // Gurkennudler
}