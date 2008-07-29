using System;
using System.Collections.Generic;
using System.Diagnostics;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Code;
using Reni.Feature;
using Reni.Parser;
using Reni.Parser.TokenClass;
using Reni.Struct;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Context
{
    /// <summary>
    /// Base class for compiler environments
    /// </summary>
    internal abstract class ContextBase : ReniObject, IDumpShortProvider, IIconKeyProvider
    {
        private static int _nextId;
        [Node, DumpData(false)]
        internal Cache Cache = new Cache();
        private Sequence<ContextBase> _childChainCache;

        protected ContextBase()
            : base(_nextId++) { }

        [Node, DumpData(false)]
        internal abstract RefAlignParam RefAlignParam { get; }
        [DumpData(false)]
        public int AlignBits { get { return RefAlignParam.AlignBits; } }
        [DumpData(false)]
        public Size RefSize { get { return RefAlignParam.RefSize; } }
        [DumpData(false)]
        internal abstract Root RootContext { get; }

        [DumpData(false)]
        internal Result TopRefResult
        {
            get
            {
                return
                    Cache._topRefResultCache.Find(
                        () => new Result {Code = CreateTopRefCode(), Refs = Refs.None()});
            }
        }
        [DumpData(false)]
        internal Sequence<ContextBase> ChildChain
        {
            get
            {
                if(_childChainCache == null)
                    _childChainCache = ObtainChildChain();
                return _childChainCache;
            }
        }

        internal protected virtual Sequence<ContextBase> ObtainChildChain() { return HWString.Sequence(this); }

        internal virtual string DumpShort() { return base.ToString(); }

        internal int SizeToPacketCount(Size size) { return size.SizeToPacketCount(RefAlignParam.AlignBits); }

        internal static Root CreateRoot() { return new Root(); }

        internal Function CreateFunction(TypeBase args)
        {
            return Cache._functionInstanceCache.Find(args,
                () => new Function(this, args));
        }

        internal Struct.FullContext CreateStruct(Struct.Container container)
        {
            return Cache._structContainerCache.Find(container,
                () => new Struct.FullContext(this, container));
        }

        internal CodeBase CreateTopRefCode() { return CodeBase.CreateTopRef(RefAlignParam); }

        internal virtual Result CreateArgsRefResult(Category category)
        {
            NotImplementedMethod(category);
            return null;
        }

        internal Result CreateFunctionResult(Category category, ICompileSyntax body) { return CreateFunctionType(body).CreateResult(category); }
        internal Result CreatePropertyResult(Category category, ICompileSyntax body) { return CreatePropertyType(body).CreateResult(category); }
        internal TypeBase CreatePropertyType(ICompileSyntax body) { return Cache._propertyType.Find(body, () => new Property(this, body)); }
        internal TypeBase CreateFunctionType(ICompileSyntax body) { return Cache._functionType.Find(body, () => new Type.Function(this, body)); }
        internal SearchResult<IContextFeature> SearchDefineable(DefineableToken defineableToken) { return Search(defineableToken.TokenClass).SubTrial(this); }
        internal virtual SearchResult<IContextFeature> Search(Defineable defineable) { return defineable.SearchContext(); }
        internal Size Size(ICompileSyntax syntax) { return Result(Category.Size, syntax).Size; }
        internal TypeBase Type(ICompileSyntax syntax) { return Result(Category.Type, syntax).Type; }

        internal List<Result> Result(Category category, List<ICompileSyntax> list)
        {
            var results = new List<Result>();
            for(var i = 0; i < list.Count; i++)
                results.Add(Result(category, list[i]));
            return results;
        }

        [DebuggerHidden]
        internal Result Result(Category category, ICompileSyntax syntax)
        {
            var cacheElem = Cache._resultCache.Find
                (
                syntax,
                () => CreateCacheElement(syntax)
                );
            return cacheElem.Result(category.Replendish());
        }

        private CacheItem CreateCacheElement(ICompileSyntax syntax)
        {
            var result = new CacheItem(syntax, this);
            syntax.AddToCache(this, result);
            return result;
        }

        internal bool IsChildOf(ContextBase parentCandidate) { return ChildChain.StartsWithAndNotEqual(parentCandidate.ChildChain); }

        internal bool IsStructParentOf(ContextBase child)
        {
            if(IsChildOf(child))
                return false;
            NotImplementedMethod(child);
            return false;
        }

        internal void AssertCorrectRefs(Result result)
        {
            if(result.HasRefs)
                AssertCorrectRefs(result.Refs);
            else
                Tracer.Assert(!result.HasCode);
        }

        private void AssertCorrectRefs(Refs refs)
        {
            foreach(var @ref in refs.Data)
                CheckRef(@ref);
        }

        private void CheckRef(IContextRefInCode @ref) { Tracer.Assert(!@ref
            .IsChildOf(this), "context=" + Dump() + "\nref=" 
            + @ref.Dump()); }

        internal BitsConst Evaluate(ICompileSyntax syntax, TypeBase resultType)
        {
            var compiledResult = Result(Category.Code | Category.Type | Category.Refs, syntax);
            var convertedResult = compiledResult.ConvertTo(resultType);
            return convertedResult.Evaluate();
        }

        internal CodeBase Code(ICompileSyntax syntax) { return Result(Category.Code, syntax).Code; }

        internal Result ApplyResult(Category category, ICompileSyntax @object,
            Result.GetResultFromType apply)
        {
            var objectResult = ResultAsRef(category | Category.Type, @object);
            return apply(objectResult.Type)
                .UseWithArg(objectResult)
                .Align(AlignBits);
        }

        internal Result ConvertToSequenceViaRef(Category category, ICompileSyntax syntax,
            TypeBase elementType, Result.GetSize argsOffset)
        {
            var applyToRef = ResultAsRef(category | Category.Type, syntax, argsOffset);
            applyToRef.AssertComplete(category | Category.Type, syntax);
            var convertTo =
                applyToRef.ConvertTo(elementType.CreateSequence(Type(syntax).SequenceCount)).Filter(
                    category);
            convertTo.AssertComplete(category);
            var result = convertTo.Align(AlignBits);
            result.AssertComplete(category);
            return result;
        }

        internal Result ResultAsRef(Category category, ICompileSyntax syntax,
            Result.GetSize argsOffset)
        {
            var localCategory = category | Category.Type;
            if(category.HasInternal)
                localCategory = localCategory | Category.ForInternal;
            return Result(localCategory, syntax)
                .EnsureRef(category | Category.Type, RefAlignParam, argsOffset)
                .Filter(category);
        }

        internal Result ResultAsRef(Category category, ICompileSyntax syntax) { return ResultAsRef(category, syntax, () => Reni.Size.Zero); }

        string IDumpShortProvider.DumpShort() { return DumpShort(); }

        /// <summary>
        /// Gets the icon key.
        /// </summary>
        /// <value>The icon key.</value>
        string IIconKeyProvider.IconKey { get { return "Context"; } }

        internal virtual IStructContext FindStruct()
        {
            NotImplementedMethod();
            return null;
        }
    }

    internal class Cache : IIconKeyProvider
    {
        [Node, SmartNode]
        internal readonly DictionaryEx<TypeBase, Function> _functionInstanceCache =
            new DictionaryEx<TypeBase, Function>();
        [Node, SmartNode]
        internal readonly DictionaryEx<ICompileSyntax, TypeBase> _functionType =
            new DictionaryEx<ICompileSyntax, TypeBase>();
        [Node, SmartNode]
        internal readonly DictionaryEx<ICompileSyntax, TypeBase> _propertyType =
            new DictionaryEx<ICompileSyntax, TypeBase>();
        [Node, SmartNode]
        internal readonly DictionaryEx<Struct.Container, Struct.FullContext> _structContainerCache =
            new DictionaryEx<Struct.Container, Struct.FullContext>();
        [Node, SmartNode]
        internal readonly SimpleCache<Result> _topRefResultCache = new SimpleCache<Result>();
        [Node, SmartNode]
        internal readonly DictionaryEx<ICompileSyntax, CacheItem> _resultCache =
            new DictionaryEx<ICompileSyntax, CacheItem>();

        /// <summary>
        /// Gets the icon key.
        /// </summary>
        /// <value>The icon key.</value>
        [DumpData(false)]
        public string IconKey { get { return "Cache"; } }
    }
}