using System;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.Helper.TreeViewSupport;
using Reni.Parser;
using Reni.Struct;
using Reni.Syntax;

namespace Reni.Context
{
    /// <summary>
    /// Base class for compiler environments
    /// </summary>
    [AdditionalNodeInfo("DebuggerDumpString")]
    public abstract class Base : ReniObject
    {
        private static int _nextId = 0;
        [Node]
        public Cache _cache = new Cache();

        /// <summary>
        /// Initializes a new instance .
        /// </summary>
        /// [created 13.05.2006 18:55]
        protected Base()
            : base(_nextId++)
        {
        }

        /// <summary>
        /// generate dump string to be shown in debug windows
        /// </summary>
        /// <returns></returns>
        public override string DebuggerDump()
        {
            return base.ToString() + " ObjectId=" + ObjectId;
        }
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
        public int PacketCountDistance(Base target)
        {
            Size d = Distance(target);
            if (d == null)
                return -1;
            return SizeToPacketCount(d);
        }

        /// <summary>
        /// Calculate the positive distance to target if possible
        /// </summary>
        /// <param name="target"></param>
        /// <returns>-1 if not based on target</returns>
        public Size Distance(Base target)
        {
            if (target == this)
                return Size.Create(0);
            return VirtDistance(target);
        }

        /// <summary>
        /// Calculate the positive distance to target if possible
        /// </summary>
        /// <param name="target"></param>
        /// <returns>-1 if not based on target</returns>
        public virtual Size VirtDistance(Base target)
        {
            DumpMethodWithBreak("not implemented", target);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Return the root env
        /// </summary>
        [DumpData(false)]
        abstract public Root RootContext{get;}

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
        /// <param name="x">The x.</param>
        /// <param name="currentCompilePosition">The currentCompilePosition.</param>
        /// <returns></returns>
        /// [created 13.05.2006 18:45]
        public Struct.Context CreateStruct(Reni.Struct.Container x, int currentCompilePosition)
        {
            return CreateStructContainer(x).CreateStruct(currentCompilePosition);
        }

        /// <summary>
        /// Gets the struct.context
        /// </summary>
        /// <param name="x">The x.</param>
        /// <returns></returns>
        /// [created 13.05.2006 18:45]
        public Struct.Context CreateStruct(Reni.Struct.Container x)
        {
            return CreateStruct(x, x.List.Count);
        }

        /// <summary>
        /// Gets the struct.context
        /// </summary>
        /// <param name="currentCompilePosition">The currentCompilePosition.</param>
        /// <returns></returns>
        /// [created 13.05.2006 18:45]
        private Struct.Context CreateStruct(int currentCompilePosition)
        {
            return _cache._structPositionCache.Find
                (
                currentCompilePosition,
                delegate { return new Struct.Context((ContainerContext)this, currentCompilePosition); }
                );
        }

        /// <summary>
        /// Creates the struct container.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <returns></returns>
        /// created 16.12.2006 14:45
        public ContainerContext CreateStructContainer(Reni.Struct.Container x)
        {
            return _cache._structContainerCache.Find(x, delegate { return new ContainerContext(this, x); });
        }
        /// <summary>
        /// Gets the function instance.
        /// </summary>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        /// [created 05.06.2006 19:22]
        public Function CreateFunction(Type.Base args)
        {
            return _cache._functionInstanceCache.Find(args, delegate { return new Function(this, args); });
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
        virtual public Result CreateArgsRefResult(Category category)
        {
            NotImplementedMethod(category);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Create a functional result
        /// </summary>
        /// <param name="c">The c.</param>
        /// <param name="body">The body.</param>
        /// <returns></returns>
        public Result CreateFunctionResult(Category c, Syntax.Base body)
        {
            return CreateFunctionType(body).CreateResult(c);
        }
        /// <summary>
        /// Creates the property result.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="body">The body.</param>
        /// <returns></returns>
        /// created 25.07.2007 22:52 on HAHOYER-DELL by hh
        public Result CreatePropertyResult(Category category, Syntax.Base body)
        {
            return CreatePropertyType(body).CreateResult(category);
        }

        /// <summary>
        /// Creates the type of the function.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <returns></returns>
        /// created 02.01.2007 14:57
        public Type.Base CreateFunctionType(Syntax.Base body)
        {
            return _cache._functionType.Find(body, delegate { return new Type.Function(this, body); });
        }

        /// <summary>
        /// Creates the type of the function.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <returns></returns>
        /// created 02.01.2007 14:57
        public Type.Base CreatePropertyType(Syntax.Base body)
        {
            Type.Base type = CreateFunctionType(body);
            return type.CreateProperty();
        }

        public class Cache
        {
            [Node]
            public DictionaryEx<Reni.Struct.Container, ContainerContext> _structContainerCache = new DictionaryEx<Reni.Struct.Container, ContainerContext>();
            [Node]
            public DictionaryEx<int, Struct.Context> _structPositionCache = new DictionaryEx<int, Struct.Context>();
            [Node]
            public DictionaryEx<Type.Base, Function> _functionInstanceCache = new DictionaryEx<Type.Base, Function>();
            [Node]
            public DictionaryEx<Syntax.Base, Type.Base> _functionType = new DictionaryEx<Syntax.Base, Type.Base>();
            [Node]
            public Result _topRefResultCache;
        }

        private Result VisitFirstChainElement(Category category, MemberElem memberElem)
        {
            if (memberElem.DefineableToken == null)
            {
                Result visitedResult = memberElem.Args.Visit(this, category);
                return visitedResult;
            }

            StructSearchResult structSearchResult = SearchDefineable(memberElem.DefineableToken);
            if (structSearchResult != null)
                return structSearchResult.VisitApply(this, category, memberElem.Args);

            if (memberElem.Args == null)
            {
                NotImplementedMethod(category, memberElem);
                return null;
            }

            Result argResult = memberElem.Args.Visit(this, category | Category.Type);
            PrefixSearchResult searchResult = argResult.Type.PrefixSearchDefineable(memberElem.DefineableToken);
            if (searchResult != null)
                return searchResult.VisitApply(category, argResult);


            NotImplementedMethod(category, memberElem);
            return null;
        }

        internal virtual StructSearchResult SearchDefineable(DefineableToken defineableToken)
        {
            NotImplementedMethod(defineableToken);
            return null;
        }

        internal Result VisitFirstChainElementAndPostProcess(Category category, MemberElem memberElem)
        {
            return PostProcess(VisitFirstChainElement(category, memberElem));
        }

        private Result PostProcess(Result formerResult)
        {
            Result alignedResult = formerResult.Align(RefAlignParam.AlignBits);
            return alignedResult.UnProperty(this);
        }

        internal Result VisitNextChainElementAndPostProcess(Category category, MemberElem memberElem, Result formerResult)
        {
            return PostProcess(VisitNextChainElement(category, memberElem, formerResult));
        }

        private Result VisitNextChainElement(Category category, MemberElem memberElem, Result formerResult)
        {
            bool trace = ObjectId == -10 && memberElem.ObjectId == 1 && category.HasAll;
            StartMethodDumpWithBreak(trace,category,memberElem,formerResult);
            Result refResult = formerResult.EnsureContextRef(this);
            Result visitedResult = refResult.Type.VisitNextChainElement(this, category, memberElem);
            Result arglessResult = visitedResult.UseWithArg(refResult);
            return ReturnMethodDumpWithBreak(trace, arglessResult);
        }

        [DumpData(false)]
        internal Result TopRefResult
        {
            get
            {
                if (_cache._topRefResultCache == null)
                {
                    _cache._topRefResultCache = new Result();
                    _cache._topRefResultCache.Code = CreateTopRefCode();
                    _cache._topRefResultCache.Refs = Refs.None();
                }

                return _cache._topRefResultCache;
            }
        }

        internal virtual bool IsChildOf(Base context)
        {
            NotImplementedMethod(context);
            return true;
        }

        internal virtual Code.Base CreateRefForStruct(Reni.Struct.Type struc)
        {
            NotImplementedMethod(struc);
            return null;
        }

        internal List<Result> Visit(Category category, List<Syntax.Base> list)
        {
            List<Result> results = new List<Result>();
            for (int i = 0; i < list.Count; i++)
                results.Add(list[i].Visit(this,category));
            return results;
        }

        internal List<Type.Base> VisitType(List<Syntax.Base> list)
        {
            List<Type.Base> results = new List<Type.Base>();
            for (int i = 0; i < list.Count; i++)
                results.Add(list[i].VisitType(this));
            return results;
        }
    }

    abstract class ___xStructSearchResult : ReniObject
    {
        readonly Struct.Context _struct;

        protected ___xStructSearchResult(Struct.Context @struct)
        {
            _struct = @struct;
        }

        protected Result VisitATApply(Base callContext, Category category, Syntax.Base args)
        {
            BitsConst indexValue = args.VisitAndEvaluate(callContext, _struct.IndexType);
            int index = indexValue.ToInt32();
            return _struct.VisitAccessApply(index, null, category, null);
        }

        internal Struct.Context Struct { get { return _struct; } }
    }
}


