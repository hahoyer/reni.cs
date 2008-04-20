using System;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.Helper.TreeViewSupport;
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
    [AdditionalNodeInfo("DebuggerDumpString")]
    internal abstract class ContextBase : ReniObject, IDumpShortProvider
    {
        private static int _nextId;

        [Node]
        internal Cache _cache = new Cache();

        /// <summary>
        /// Initializes a new instance .
        /// </summary>
        /// [created 13.05.2006 18:55]
        protected ContextBase()
            : base(_nextId++) {}

        /// <summary>
        /// Parameter to describe alignment for references
        /// </summary>
        [Node, DumpData(false)]
        public abstract RefAlignParam RefAlignParam { get; }

        /// <summary>
        /// Provide the number of bits for data alingment. For example 8 for byte alignement
        /// </summary>
        [DumpData(false)]
        public int AlignBits { get { return RefAlignParam.AlignBits; } }

        /// <summary>
        /// Provide the size of a reference
        /// </summary>
        [DumpData(false)]
        public Size RefSize { get { return RefAlignParam.RefSize; } }

        /// <summary>
        /// Return the root env
        /// </summary>
        [DumpData(false)]
        public abstract Root RootContext { get; }

        [DumpData(false)]
        internal Result TopRefResult
        {
            get
            {
                if(_cache._topRefResultCache == null)
                    _cache._topRefResultCache = new Result {Code = CreateTopRefCode(), Refs = Refs.None()};

                return _cache._topRefResultCache;
            }
        }

        #region IDumpShortProvider Members

        public string DumpShort()
        {
            return DebuggerDump();
        }

        #endregion

        /// <summary>
        /// generate dump string to be shown in debug windows
        /// </summary>
        /// <returns></returns>
        public override string DebuggerDump()
        {
            return base.ToString() + " ObjectId=" + ObjectId;
        }

        /// <summary>
        /// Convert size into packets by use of align bits. 
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public int SizeToPacketCount(Size size)
        {
            return size.SizeToPacketCount(RefAlignParam.AlignBits);
        }

        /// <summary>
        /// Calculate the positive distance to target if possible
        /// </summary>
        /// <param name="target"></param>
        /// <returns>-1 if not based on target</returns>
        public int PacketCountDistance(ContextBase target)
        {
            var d = Distance(target);
            if(d == null)
                return -1;
            return SizeToPacketCount(d);
        }

        /// <summary>
        /// Calculate the positive distance to target if possible
        /// </summary>
        /// <param name="target"></param>
        /// <returns>-1 if not based on target</returns>
        public Size Distance(ContextBase target)
        {
            if(target == this)
                return Size.Create(0);
            return VirtDistance(target);
        }

        /// <summary>
        /// Calculate the positive distance to target if possible
        /// </summary>
        /// <param name="target"></param>
        /// <returns>-1 if not based on target</returns>
        public virtual Size VirtDistance(ContextBase target)
        {
            DumpMethodWithBreak("not implemented", target);
            throw new NotImplementedException();
        }

        /// <summary>
        /// creates the root environment
        /// </summary>
        public static Root CreateRoot()
        {
            return new Root();
        }

        /// <summary>
        /// Gets the struct.context
        /// </summary>
        /// <param name="container">The x.</param>
        /// <param name="currentCompilePosition">The currentCompilePosition.</param>
        /// <returns></returns>
        /// [created 13.05.2006 18:45]
        internal ContextAtPosition CreateStructAtPosition(Struct.Container container, int currentCompilePosition)
        {
            return CreateStructContext(container).CreateStructAtPosition(currentCompilePosition);
        }

        /// <summary>
        /// Gets the struct.context
        /// </summary>
        /// <param name="currentCompilePosition">The currentCompilePosition.</param>
        /// <returns></returns>
        /// [created 13.05.2006 18:45]
        private ContextAtPosition CreateStructAtPosition(int currentCompilePosition)
        {
            return _cache._structPositionCache.Find
                (
                currentCompilePosition,
                () => new ContextAtPosition((Struct.Context) this, currentCompilePosition)
                );
        }

        /// <summary>
        /// Creates the struct container.
        /// </summary>
        /// <param name="container">The x.</param>
        /// <returns></returns>
        /// created 16.12.2006 14:45
        internal Struct.Context CreateStructContext(Struct.Container container)
        {
            return _cache._structContainerCache.Find(container,
                () => new Struct.Context(this, container));
        }

        /// <summary>
        /// Gets the function instance.
        /// </summary>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        /// [created 05.06.2006 19:22]
        public Function CreateFunction(TypeBase args)
        {
            return _cache._functionInstanceCache.Find(args,
                () => new Function(this, args));
        }

        /// <summary>
        /// Creates the top ref code.
        /// </summary>
        /// <returns></returns>
        /// [created 01.06.2006 22:56]
        internal Code.Base CreateTopRefCode()
        {
            return Code.Base.CreateTopRef(RefAlignParam);
        }

        /// <summary>
        /// Creates the args ref result.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        /// created 03.11.2006 22:00
        public virtual Result CreateArgsRefResult(Category category)
        {
            NotImplementedMethod(category);
            return null;
        }

        /// <summary>
        /// Create a functional result
        /// </summary>
        /// <param name="category">The c.</param>
        /// <param name="body">The body.</param>
        /// <returns></returns>
        internal Result CreateFunctionResult(Category category, SyntaxBase body)
        {
            return CreateFunctionType(body).CreateResult(category);
        }

        internal Result CreatePropertyResult(Category category, SyntaxBase body)
        {
            return CreatePropertyType(body).CreateResult(category);
        }

        internal TypeBase CreatePropertyType(SyntaxBase body)
        {
            return _cache._propertyType.Find(body,
                () => new Property(this, body));
        }

        /// <summary>
        /// Creates the type of the function.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <returns></returns>
        /// created 02.01.2007 14:57
        public TypeBase CreateFunctionType(SyntaxBase body)
        {
            return _cache._functionType.Find(body,
                () => new Type.Function(this, body));
        }

        internal SearchResult<IContextFeature> SearchDefineable(DefineableToken defineableToken)
        {
            return Search(defineableToken.TokenClass).SubTrial(this);
        }

        internal virtual SearchResult<IContextFeature> Search(Defineable defineable)
        {
            return defineable.SearchContext();
        }

        internal Result VisitFirstChainElement(Category category, MemberElem memberElem)
        {
            if(memberElem.DefineableToken == null)
                return memberElem.Args.Visit(this, category);

            var contextSearchResult = SearchDefineable(memberElem.DefineableToken);
            if(contextSearchResult.IsSuccessFull)
                return contextSearchResult.Feature.VisitApply(this, category, memberElem.Args);

            if(memberElem.Args == null)
            {
                NotImplementedMethod(category, memberElem, "contextSearchResult", contextSearchResult);
                return null;
            }

            var argResult = memberElem.Args.Visit(this, category | Category.Type);
            var searchResult = argResult.Type.SearchDefineablePrefix(memberElem.DefineableToken);
            if(searchResult.IsSuccessFull)
                return searchResult.Feature.VisitApply(category, argResult);

            NotImplementedMethod(category, memberElem, "searchResult", searchResult);
            return null;
        }

        internal Result VisitNextChainElement(Category category, MemberElem memberElem, Result formerResult)
        {
            var trace = ObjectId == -3 && memberElem.ObjectId == 1 && category.HasAll;
            StartMethodDumpWithBreak(trace, category, memberElem, formerResult);
            var refResult = formerResult.EnsureContextRef(this);
            var visitedResult = ((Ref) refResult.Type).VisitNextChainElement(this, category, memberElem);
            Tracer.Assert(visitedResult != null);
            var arglessResult = visitedResult.UseWithArg(refResult);
            return ReturnMethodDumpWithBreak(trace, arglessResult);
        }

        internal virtual bool IsChildOf(ContextBase context)
        {
            NotImplementedMethod(context);
            return true;
        }

        internal virtual Code.Base CreateRefForStruct(Struct.Type type)
        {
            NotImplementedMethod(type);
            return null;
        }

        internal List<Result> Visit(Category category, List<SyntaxBase> list)
        {
            var results = new List<Result>();
            for(var i = 0; i < list.Count; i++)
                results.Add(list[i].Visit(this, category));
            return results;
        }

        internal List<TypeBase> VisitType(List<SyntaxBase> list)
        {
            var results = new List<TypeBase>();
            for(var i = 0; i < list.Count; i++)
                results.Add(list[i].VisitType(this));
            return results;
        }

        #region Nested type: Cache

        internal class Cache
        {
            [Node]
            internal DictionaryEx<TypeBase, Function> _functionInstanceCache =
                new DictionaryEx<TypeBase, Function>();

            [Node]
            internal DictionaryEx<SyntaxBase, TypeBase> _functionType =
                new DictionaryEx<SyntaxBase, TypeBase>();

            [Node]
            internal DictionaryEx<SyntaxBase, TypeBase> _propertyType =
                new DictionaryEx<SyntaxBase, TypeBase>();

            [Node]
            internal DictionaryEx<Container, Struct.Context> _structContainerCache =
                new DictionaryEx<Container, Struct.Context>();

            [Node]
            internal DictionaryEx<int, ContextAtPosition> _structPositionCache =
                new DictionaryEx<int, ContextAtPosition>();

            [Node]
            internal Result _topRefResultCache;
        }

        #endregion
    }
}