using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.Helper.TreeViewSupport;
using Reni.Context;
using Reni.Feature;
using Reni.Parser;
using Reni.Parser.TokenClass;
using Reni.Syntax;

namespace Reni.Type
{
    [AdditionalNodeInfo("DebuggerDumpString")]
    internal abstract class TypeBase : ReniObject, IDumpShortProvider
    {
        private static readonly Bit _bit = new Bit();
        private static readonly Void _void = new Void();
        private static readonly Pending _pending = new Pending();
        private readonly DictionaryEx<int, Aligner> _aligner = new DictionaryEx<int, Aligner>();
        private readonly DictionaryEx<int, Array> _array = new DictionaryEx<int, Array>();
        private readonly DictionaryEx<int, Sequence> _chain = new DictionaryEx<int, Sequence>();
        private readonly DictionaryEx<TypeBase, Pair> _pair = new DictionaryEx<TypeBase, Pair>();
        private readonly DictionaryEx<RefAlignParam, Ref> _ref = new DictionaryEx<RefAlignParam, Ref>();
        private TypeType _typeTypeCache;

        protected TypeBase(int objectId)
            : base(objectId) {}

        protected TypeBase() {}

        internal static TypeBase CreateVoid { get { return _void; } }
        internal static TypeBase CreateBit { get { return _bit; } }
        internal static TypeBase Pending { get { return _pending; } }

        [Node]
        internal abstract Size Size { get; }
        [DumpData(false)]
        internal virtual bool IsRef { get { return false; } }
        [DumpData(false)]
        internal virtual bool IsVoid { get { return false; } }
        [DumpData(false)]
        internal virtual Size UnrefSize { get { return Size; } }
        [DumpData(false)]
        internal virtual bool IsPending { get { return false; } }
        [DumpData(false)]
        internal protected virtual TypeBase[] ToList { get { return new[] { this }; } }

        private TypeBase TypeType
        {
            get
            {
                if(_typeTypeCache == null)
                    _typeTypeCache = new TypeType(this);
                return _typeTypeCache;
            }
        }

        [DumpData(false)]
        internal virtual string DumpPrintText
        {
            get
            {
                NotImplementedMethod();
                throw new NotImplementedException();
            }
        }

        [DumpData(false)]
        public virtual bool HasEmptyValue()
        {
            NotImplementedMethod();
            throw new NotImplementedException();
        }

        [DumpData(false)]
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
                throw new NotImplementedException();
            }
        }


        public string DumpShort()
        {
            return DumpPrintText;
        }

        public TypeBase CreateAlign(int alignBits)
        {
            if(Size.Align(alignBits) == Size)
                return this;
            return _aligner.Find(alignBits, () => new Aligner(this, alignBits));
        }

        public Array CreateArray(int count)
        {
            return _array.Find(count, () => new Array(this, count));
        }

        public static TypeBase CreateNumber(int bitCount)
        {
            return CreateBit.CreateSequence(bitCount);
        }

        public virtual TypeBase CreatePair(TypeBase second)
        {
            return second.CreateReversePair(this);
        }

        protected virtual TypeBase CreateReversePair(TypeBase first)
        {
            return first._pair.Find(this,
                () => new Pair(first, this));
        }

        public virtual Ref CreateRef(RefAlignParam refAlignParam)
        {
            return _ref.Find(refAlignParam, () => new Ref(this, refAlignParam));
        }

        public Sequence CreateSequence(int elementCount)
        {
            return _chain.Find(elementCount, () => new Sequence(this, elementCount));
        }

        internal virtual Result DestructorHandler(Category category)
        {
            NotImplementedMethod(category);
            throw new NotImplementedException();
        }

        internal virtual Result ArrayDestructorHandler(Category category, int count)
        {
            NotImplementedMethod(category, count);
            throw new NotImplementedException();
        }

        internal virtual Result MoveHandler(Category category)
        {
            NotImplementedMethod(category);
            throw new NotImplementedException();
        }

        internal virtual Result ArrayMoveHandler(Category category, int count)
        {
            NotImplementedMethod(category, count);
            throw new NotImplementedException();
        }

        internal Result CreateArgResult(Category category)
        {
            return CreateResult(category, CreateArgCode);
        }

        internal Code.CodeBase CreateArgCode()
        {
            return Code.CodeBase.CreateArg(Size);
        }

        internal Result CreateResult(Category category)
        {
            return CreateResult(category,
                () => Code.CodeBase.CreateBitArray(Size, BitsConst.Convert(0).Resize(Size)));
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

        internal Result CreateResult(Category category, Result.GetCode getCode)
        {
            return CreateResult(category, getCode, Refs.None);
        }

        public Result CreateContextRefResult<C>(Category category, C context) where C : ContextBase
        {
            return CreateResult(
                category,
                () => Code.CodeBase.CreateContextRef(context),
                () => Refs.Context(context));
        }

        internal Result CreateResult(Category category, Result.GetCode getCode, Result.GetRefs getRefs)
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

        internal virtual Result ApplyFunction(Category category, ContextBase callContext, SyntaxBase args)
        {
            NotImplementedMethod(callContext, category, args);
            throw new NotImplementedException();
        }

        internal virtual Result ApplyFunction(Category category, Result argsResult)
        {
            NotImplementedMethod(category, argsResult);
            return null;
        }

        internal virtual Result PostProcess(Ref visitedType, Result result)
        {
            if(this == visitedType.Target)
                return result.UseWithArg(visitedType.CreateDereferencedArgResult(result.Complete));
            NotImplementedMethod(visitedType, result);
            return null;
        }

        public static Result EmptyHandler(Category category)
        {
            return CreateVoidResult(category - Category.Type - Category.Size);
        }

        public static Result CreateVoidResult(Category category)
        {
            return CreateVoid.CreateResult(category);
        }

        public virtual Result Dereference(Result result)
        {
            return result;
        }

        public virtual Result TypeOperator(Category category)
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
            var argResult = CreateRef(refAlignParam).Conversion(category, this);
            return DumpPrint(category).UseWithArg(argResult);
        }

        public virtual Result ArrayDumpPrint(Category category, int count)
        {
            NotImplementedMethod(category, count);
            throw new NotImplementedException();
        }

        internal virtual Result SequenceDumpPrint(Category category, int count)
        {
            NotImplementedMethod(category, count);
            throw new NotImplementedException();
        }

        public virtual TypeBase DumpPrintArrayType(int count)
        {
            NotImplementedMethod(count);
            throw new NotImplementedException();
        }

        public virtual Result ApplyTypeOperator(Result argResult)
        {
            return argResult.Type.Conversion(argResult.Complete, this).UseWithArg(argResult);
        }

        public TypeBase CommonType(TypeBase dest)
        {
            if(IsConvertableTo(dest, ConversionFeature.Instance))
                return dest;
            if(dest.IsConvertableTo(this, ConversionFeature.Instance))
                return this;
            NotImplementedMethod(dest);
            throw new NotImplementedException();
        }

        public Result Conversion(Category category, TypeBase dest)
        {
            if(category.HasCode || category.HasRefs)
            {
                if(IsConvertableTo(dest, ConversionFeature.Instance))
                    return ConvertTo(category, dest);
                NotImplementedMethod(category, dest);
                throw new NotImplementedException();
            }
            return dest.CreateResult(category);
        }

        internal Result ConvertTo(Category category, TypeBase dest)
        {
            if(this == dest)
                return ConvertToItself(category);
            return ConvertToVirt(category, dest);
        }

        internal virtual Result ConvertToItself(Category category)
        {
            return CreateArgResult(category);
        }

        internal virtual Result ConvertToVirt(Category category, TypeBase dest)
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
            return IsConvertableToVirt(dest, conversionFeature);
        }

        internal virtual bool HasConverterTo(TypeBase dest)
        {
            return false;
        }

        internal virtual bool IsConvertableToVirt(TypeBase dest, ConversionFeature conversionFeature)
        {
            NotImplementedMethod(dest, conversionFeature);
            throw new NotImplementedException();
        }

        internal virtual bool IsConvertableToItself(ConversionFeature conversionFeature)
        {
            return true;
        }

        internal virtual Code.CodeBase CreateSequenceOperation(Defineable token, Result objResult, Size size,
            Result argResult)
        {
            NotImplementedMethod(token, objResult, size, argResult.Code);
            return null;
        }

        internal virtual Code.CodeBase CreateSequenceOperation(Defineable token, Result result)
        {
            NotImplementedMethod(token, result);
            return null;
        }

        internal virtual TypeBase SequenceOperationResultType(Defineable token, int objBitCount, int argBitCount)
        {
            NotImplementedMethod(token, objBitCount, argBitCount);
            return null;
        }

        internal Result ThenElseWithPending(Category category, Refs condRefs, Refs elseOrThenRefs)
        {
            Tracer.Assert(!category.HasCode);

            return CreateResult
                (
                category,
                () => null,
                () => condRefs.Pair(elseOrThenRefs)
                );
        }

        internal virtual Code.CodeBase CreateRefCodeForContext(ContextBase context)
        {
            NotImplementedMethod(context);
            return null;
        }

        internal virtual Result UnProperty(Result rawResult, ContextBase context)
        {
            return rawResult;
        }

        internal SearchResult<IFeature> SearchDefineable(DefineableToken defineableToken)
        {
            return Search(defineableToken.TokenClass).SubTrial(this);
        }
        internal SearchResult<IPrefixFeature> SearchDefineablePrefix(DefineableToken defineableToken)
        {
            return SearchPrefix(defineableToken.TokenClass).SubTrial(this);
        }

        internal protected virtual SearchResult<IFeature> Search(Defineable defineable)
        {
            return defineable.Search();
        }
        internal protected virtual SearchResult<IPrefixFeature> SearchPrefix(Defineable defineable)
        {
            return defineable.SearchPrefix();
        }

        internal virtual SearchResult<IRefFeature> SearchFromRef(Defineable defineable)
        {
            return defineable.SearchFromRef();
        }

        internal virtual SearchResult<ISequenceElementFeature> SearchFromSequence(Defineable defineable)
        {
            return defineable.SearchFromSequenceElement();
        }
        internal virtual SearchResult<ISequenceElementPrefixFeature> SearchPrefixFromSequence(Defineable defineable)
        {
            return defineable.SearchPrefixFromSequenceElement();
        }

        internal virtual SearchResult<IRefToSequenceFeature> SearchFromRefToSequence(Defineable defineable)
        {
            return SearchResult<IRefToSequenceFeature>.Failure(this, defineable);
        }

    }

}