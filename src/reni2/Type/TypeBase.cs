using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.Helper.TreeViewSupport;
using Reni.Code;
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
        private readonly DictionaryEx<RefAlignParam, AutomaticRef> _ref = new DictionaryEx<RefAlignParam, AutomaticRef>();
        private readonly DictionaryEx<RefAlignParam, AssignableRef> _assignableRef = new DictionaryEx<RefAlignParam, AssignableRef>();

        private readonly SimpleCache<TypeType> _typeTypeCache = new SimpleCache<TypeType>();

        protected TypeBase(int objectId)
            : base(objectId) {}

        protected TypeBase() {}

        internal static TypeBase CreateVoid { get { return _void; } }
        internal static TypeBase CreateBit { get { return _bit; } }
        internal static TypeBase Pending { get { return _pending; } }

        [Node]
        internal abstract Size Size { get; }

        internal virtual bool IsRef(RefAlignParam refAlignParam)
        {
            return false;
        }

        [DumpData(false)]
        internal virtual bool IsVoid { get { return false; } }
        [DumpData(false)]
        internal virtual Size UnrefSize { get { return Size; } }
        [DumpData(false)]
        internal virtual bool IsPending { get { return false; } }
        [DumpData(false)]
        internal protected virtual TypeBase[] ToList { get { return new[] {this}; } }

        string IDumpShortProvider.DumpShort()
        {
            return DumpShort();
        }

        internal abstract string DumpShort();

        private TypeBase TypeType { get { return _typeTypeCache.Find(() => new TypeType(this)); } }

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
                return 0;
            }
        }

        [DumpData(false)]
        internal TypeBase IndexType
        {
            get
            {
                return CreateNumber(IndexSize);
            }
        }

        [DumpData(false)]
        internal protected virtual int IndexSize
        {
            get
            {
                NotImplementedMethod();
                return 0;
            }
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

        public virtual AutomaticRef CreateRef(RefAlignParam refAlignParam)
        {
            return _ref.Find(refAlignParam, () => new AutomaticRef(this, refAlignParam));
        }

        public virtual AssignableRef CreateAssignableRef(RefAlignParam refAlignParam)
        {
            return _assignableRef.Find(refAlignParam, () => new AssignableRef(this, refAlignParam));
        }

        public AutomaticRef EnsureRef(RefAlignParam refAlignParam)
        {
            if(IsRef(refAlignParam))
                return (AutomaticRef) this;
            return CreateRef(refAlignParam);
        }

        public Sequence CreateSequence(int elementCount)
        {
            return _chain.Find(elementCount, () => new Sequence(this, elementCount));
        }

        internal virtual Result DestructorHandler(Category category)
        {
            return EmptyHandler(category);
        }

        internal virtual Result ArrayDestructorHandler(Category category, int count)
        {
            return EmptyHandler(category);
        }

        internal virtual Result MoveHandler(Category category)
        {
            return EmptyHandler(category);
        }

        internal virtual Result ArrayMoveHandler(Category category, int count)
        {
            return EmptyHandler(category);
        }

        internal Result CreateArgResult(Category category)
        {
            return CreateResult(category, CreateArgCode);
        }

        internal CodeBase CreateArgCode()
        {
            return CodeBase.CreateArg(Size);
        }

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
            if(category.HasInternal)
                result.Internal = codeAndRefs.Internal;
            return result;
        }

        internal Result CreateResult(Category category, Result.GetCode getCode)
        {
            return CreateResult(category, getCode, Refs.None, Result.EmptyInternal);
        }

        internal Result CreateResult(Category category, Result.GetCode getCode, Result.GetResult getInternal)
        {
            return CreateResult(category, getCode, Refs.None, getInternal);
        }

        public Result CreateContextRefResult<C>(Category category, C context) where C : ContextBase
        {
            return CreateResult(
                category,
                () => CodeBase.CreateContextRef(context),
                () => Refs.Context(context));
        }

        internal Result CreateResult(Category category, Result.GetCode getCode, Result.GetRefs getRefs)
        {
            return CreateResult(category, getCode, getRefs, Result.EmptyInternal);
        }

        internal Result CreateResult(Category category, Result.GetCode getCode, Result.GetRefs getRefs, Result.GetResult getInternal)
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
            if(category.HasInternal)
                result.Internal = getInternal();
            return result;
        }

        internal virtual Result ApplyFunction(Category category, ContextBase callContext, ICompileSyntax args)
        {
            NotImplementedMethod(callContext, category, args);
            throw new NotImplementedException();
        }

        internal virtual Result ApplyFunction(Category category, Result argsResult)
        {
            NotImplementedMethod(category, argsResult);
            return null;
        }

        internal virtual Result PostProcess(AutomaticRef visitedType, Result result)
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

        internal virtual Result AutomaticDereference(Result result)
        {
            return result;
        }

        internal virtual TypeBase AutomaticDereference()
        {
            return this;
        }

        internal virtual Result TypeOperator(Category category)
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

        internal Result ApplyTypeOperator(Category category, TypeBase targetType)
        {
            return targetType.Conversion(category, this);
        }

        internal virtual Result ApplyTypeOperator(Result argResult)
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

        internal virtual CodeBase CreateSequenceOperation(Size size, Defineable token, Size objSize, Size argsSize)
        {
            NotImplementedMethod(size, token, objSize, argsSize);
            return null;
        }

        internal virtual CodeBase CreateSequenceOperation(Defineable token, Result result)
        {
            NotImplementedMethod(token, result);
            return null;
        }

        internal protected virtual TypeBase SequenceOperationResultType(Defineable token, int objBitCount, int argBitCount)
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

        internal virtual CodeBase CreateRefCodeForContext(ContextBase context)
        {
            NotImplementedMethod(context);
            return null;
        }

        internal virtual Result UnProperty(Result rawResult)
        {
            return rawResult;
        }

        internal virtual TypeBase UnProperty()
        {
            return this;
        }

        internal SearchResult<IFeature> SearchDefineable(DefineableToken defineableToken)
        {
            return Search(defineableToken.TokenClass).SubTrial(this);
        }

        internal SearchResult<IPrefixFeature> SearchDefineablePrefix(DefineableToken defineableToken)
        {
            return SearchPrefix(defineableToken.TokenClass).SubTrial(this);
        }

        internal virtual SearchResult<IFeature> Search(Defineable defineable)
        {
            return defineable.Search().SubTrial(this);
        }

        internal protected virtual SearchResult<IPrefixFeature> SearchPrefix(Defineable defineable)
        {
            return defineable.SearchPrefix().SubTrial(this);
        }

        internal virtual SearchResult<IConverter<IFeature, Ref>> SearchFromRef(Defineable defineable)
        {
            return defineable.SearchFromRef().SubTrial(this);
        }

        internal virtual SearchResult<IConverter<IFeature, Sequence>> SearchFromSequence(Defineable defineable)
        {
            return defineable.SearchFromSequenceElement().SubTrial(this);
        }

        internal virtual SearchResult<IConverter<IPrefixFeature, Sequence>> SearchPrefixFromSequence(Defineable defineable)
        {
            return defineable.SearchPrefixFromSequenceElement().SubTrial(this);
        }

        internal virtual SearchResult<IConverter<IConverter<IFeature, Ref>, Sequence>> SearchFromRefToSequence(Defineable defineable)
        {
            return SearchResult<IConverter<IConverter<IFeature, Ref>, Sequence>>.Failure(this, defineable);
        }

        internal Result ConvertToSequence(ContextBase callContext, Category category, ICompileSyntax args)
        {
            var result = callContext.Result(category | Category.Type, args);
            return result.ConvertTo(CreateSequence(result.Type.SequenceCount));
        }

        internal Result SequenceOperationResult(Category category, Defineable definable, Size objSize, Size argsSize)
        {
            var type = SequenceOperationResultType(definable, objSize.ToInt(), argsSize.ToInt());
            return type
                .CreateResult(category, () => CreateSequenceOperation(type.Size, definable, objSize, argsSize));
        }

        internal virtual Result AccessResultFromRef(Category category, int index, RefAlignParam refAlignParam)
        {
            NotImplementedMethod(category, index, refAlignParam);
            return null;
        }
        internal virtual Result AccessResult(Category category, int index)
        {
            NotImplementedMethod(category, index);
            return null;
        }
    }
}