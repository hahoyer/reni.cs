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
using Reni.Syntax;

namespace Reni.Type
{
    [Serializable]
    abstract class TypeBase
        : ReniObject
          , IContextReference
          , IDumpShortProvider
          , IIconKeyProvider
          , ISearchTarget
          , ISearchPath<ISuffixFeature, Aligner>, ISearchPath<ISuffixFeature, TypeBase>
    {
        sealed class Cache
        {
            public static readonly Bit Bit = new Bit();
            public static readonly Void Void = new Void();
            public readonly DictionaryEx<int, Aligner> Aligner;
            public readonly DictionaryEx<int, Array> Array;
            public readonly DictionaryEx<TypeBase, Pair> Pair;
            public readonly SimpleCache<ISmartReference> Pointer;
            public readonly DictionaryEx<int, ReferenceType> Reference;
            public readonly SimpleCache<TypeType> TypeType;
            public readonly SimpleCache<FunctionInstanceType> FunctionInstanceType;
            public readonly SimpleCache<TextItemType> TextItem;
            public readonly SimpleCache<EnableCut> EnableCut;

            public Cache(TypeBase parent)
            {
                EnableCut = new SimpleCache<EnableCut>(() => new EnableCut(parent));
                Pointer = new SimpleCache<ISmartReference>(parent.ObtainPointer);
                Reference = new DictionaryEx<int, ReferenceType>(parent.ObtainReference);
                Pair = new DictionaryEx<TypeBase, Pair>(first => new Pair(first, parent));
                Array = new DictionaryEx<int, Array>(parent.ObtainArray);
                Aligner = new DictionaryEx<int, Aligner>(alignBits => new Aligner(parent, alignBits));
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
        internal virtual TypeBase[] ToList { get { return new[] {this}; } }

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
            if(IsConvertable(elementType))
                return 1;
            NotImplementedMethod(elementType);
            return null;
        }

        [DisableDump]
        internal TextItemType UniqueTextItemType { get { return _cache.TextItem.Value; } }

        [DisableDump]
        internal EnableCut UniqueEnableCutType { get { return _cache.EnableCut.Value; } }

        [DisableDump]
        internal TypeBase UniquePointer { get { return UniqueSmartReference.Type(); } }
        [DisableDump]
        internal virtual ISmartReference UniqueSmartReference { get { return _cache.Pointer.Value; } }

        [DisableDump]
        internal virtual ReferenceType SmartReference { get { return _cache.Reference.Value(1); } }
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

        [DisableDump]
        internal TypeBase SmartPointer
        {
            get
            {
                if(IsDataLess)
                    return this;
                return UniquePointer;
            }
        }

        [DisableDump]
        internal TypeBase UniqueAlign
        {
            get
            {
                var alignBits = Root.DefaultRefAlignParam.AlignBits;
                if(Size.Align(alignBits) == Size)
                    return this;
                return _cache.Aligner.Value(alignBits);
            }
        }

        internal ReferenceType UniqueReference(int count) { return _cache.Reference.Value(count); }
        internal Array UniqueArray(int count) { return _cache.Array.Value(count); }
        protected virtual TypeBase ReversePair(TypeBase first) { return first._cache.Pair.Value(this); }
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
        internal Result PointerArgResult(Category category) { return UniquePointer.ArgResult(category); }
        internal Result PointerResult(Category category, Func<Category, Result> getCodeAndRefs) { return UniquePointer.Result(category, getCodeAndRefs); }

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
        internal virtual ISmartReference Reference { get { return this as ISmartReference; } }

        [DisableDump]
        internal virtual IFeature Feature { get { return null; } }

        [DisableDump]
        protected virtual ISearchTarget ConversionProvider { get { return this; } }
        [DisableDump]
        internal virtual bool HasQuickSize { get { return true; } }
        [DisableDump]
        internal virtual TypeBase CoreType { get { return this; } }

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
            if(length != null)
                return length.Value;

            NotImplementedMethod(elementType);
            return -1;
        }

        internal SearchResult Search<TFeature>(ISearchTarget target)
            where TFeature : class, IFeature
        {
            var visitor = new TypeRootSearchVisitor<TFeature>(target, this);
            var former = SearchVisitor.Trace;
            SearchVisitor.Trace = false;
            Search(visitor);
            SearchVisitor.Trace = former;
            if(Debugger.IsAttached && !visitor.IsSuccessFull)
                _lastSearchVisitor = visitor;
            return visitor.SearchResult;
        }

        internal virtual void Search(SearchVisitor searchVisitor) { searchVisitor.Search(this, null); }

        internal virtual Result SmartLocalReferenceResult(Category category)
        {
            if(IsDataLess)
                return ArgResult(category);

            return UniquePointer
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
            return UniquePointer
                .Result(category, target);
        }

        internal Result ContextAccessResult(Category category, IContextReference target, Size offset)
        {
            if(IsDataLess)
                return Result(category);
            return new Result
                (category
                 , getType: () => this
                 , getCode: () => CodeBase.ReferenceCode(target).ReferencePlus(offset).Dereference(Size)
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

        ISmartReference ObtainPointer() { return this as ISmartReference ?? new PointerType(this); }

        ReferenceType ObtainReference(int count) { return new ReferenceType(AutomaticDereferenceType, count); }

        protected virtual Array ObtainArray(int count) { return new Array(this, count); }

        internal CodeBase BitSequenceOperation(ISequenceOfBitPrefixOperation token)
        {
            return UniqueAlign.ArgCode
                .BitSequenceOperation(token, Size);
        }

        internal Result GenericDumpPrintResult(Category category) { return OperationResult<ISuffixFeature>(category, _dumpPrintToken.Value); }

        static readonly SimpleCache<DumpPrintToken> _dumpPrintToken =
            new SimpleCache<DumpPrintToken>(DumpPrintToken.Create);

        internal Result CreateArray(Category category)
        {
            return UniqueAlign
                .UniqueArray(1).UniquePointer
                .Result(category, PointerArgResult(category));
        }

        internal bool IsConvertable(TypeBase destination)
        {
            return CoreType == destination.CoreType
                   || Converter(destination) != null;
        }

        internal Result Conversion(Category category, TypeBase destination)
        {
            if(category <= (Category.Type.Replenished))
                return destination.SmartReference.Result(category);

            var obviousConversionResult = ObviousConversion(category.Typed, destination);
            if(obviousConversionResult != null)
                return obviousConversionResult;

            var searchResult = Converter(destination);
            if(searchResult == null)
            {
                NotImplementedMethod(category, destination);
                return null;
            }

            var result = searchResult.Result(category.Typed);
            return result
                       .Type
                       .ObviousConversion(category, destination)
                       .ReplaceArg(result)
                   & category;
        }

        Result ObviousConversion(Category category, TypeBase destination)
        {
            if(CoreType != destination.CoreType)
                return null;

            if(Reference == null)
                return SmartLocalReferenceResult(category);

            return ArgResult(category);
        }

        internal Result ObviousExactConversion(Category category, TypeBase destination)
        {
            if(CoreType != destination.CoreType)
                return null;

            if(Reference == null && destination.Reference != null)
                return SmartLocalReferenceResult(category);

            var result = ArgResult(category);

            if(this != destination)
            {
                if(Reference != null)
                    result = result.DereferenceResult();

                if(destination is Aligner)
                    result = result.Align(Root.DefaultRefAlignParam.AlignBits);
                else if(Reference == null)
                    result = result.UnalignedResult();
            }

            return result;
        }

        SearchResult Converter(TypeBase destination)
        {
            return
                Search<ISuffixFeature>(destination.ConversionProvider);
        }

        internal Result TextItemResult(Category category)
        {
            var uniqueTextItem = UniqueTextItemType;
            return
                uniqueTextItem.UniquePointer
                    .Result(category, UniquePointer.ArgResult(category))
                ;
        }

        internal virtual Result ConstructorResult(Category category, TypeBase argsType) { return argsType.Conversion(category, this); }

        internal Result ConcatArrayFromReference(Category category, PointerType pointerType)
        {
            NotImplementedMethod(category, pointerType);
            return null;
        }

        Result DereferenceReferenceResult(Category category)
        {
            return UniquePointer
                .ArgResult(category.Typed)
                .DereferenceResult();
        }

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

        ISuffixFeature ISearchPath<ISuffixFeature, Aligner>.Convert(Aligner type)
        {
            if (type.Parent == this)
                return Extension.Feature(PointerArgResult);
            return null;
        }

        ISuffixFeature ISearchPath<ISuffixFeature, TypeBase>.Convert(TypeBase type) { return Convert(type); }

        protected virtual ISuffixFeature Convert(TypeBase type)
        {
            if(type.CoreType == CoreType)
                return Extension.Feature(DereferenceReferenceResult);

            return null;
        }

        internal TypeBase SmartUn<T>()
            where T : IConverter { return this is T ? ((IConverter) this).Result(Category.Type).Type : this; }

        internal Result ReferenceConversionResult(Category category, TypeBase destinationType)
        {
            return destinationType
                .UniquePointer
                .Result
                (category
                 , UniquePointer.ArgResult(category.Typed));
        }

        internal static Result Result(Category category, BitsConst bitsConst)
        {
            return UniqueNumber(bitsConst.Size.ToInt())
                .Result(category, getCode: () => CodeBase.BitsConst(bitsConst));
        }

        internal virtual Result InstanceResult(Category category, Func<Category, Result> getRightResult)
        {
            NotImplementedMethod(category, getRightResult(Category.All));
            return null;
        }
        internal Result CreateReference(ContextBase context, Category category, CompileSyntax target)
        {
            var rawResult = SmartReference.ArgResult(category);
            if(category <= Category.Type.Replenished)
                return rawResult;

            var targetResult = target.SmartReferenceResult(context, category.Typed);
            var convertedResult = targetResult.Conversion(SmartReference);
            return rawResult.ReplaceArg(convertedResult);
        }
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