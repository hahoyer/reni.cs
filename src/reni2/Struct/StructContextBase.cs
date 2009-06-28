using HWClassLibrary.TreeStructure;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using System;
using HWClassLibrary.Helper;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Parser.TokenClass;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Struct
{
    [Serializable]
    internal abstract class StructContextBase : ContextBase, IStructContext
    {
        private readonly SimpleCache<PositionFeature[]> _featuresCache;
        [Node]
        internal readonly ContextBase Parent;
        [Node]
        internal readonly Container Container;
        [Node, DumpData(false)]
        private readonly Result[] _internalResult;
        
        protected StructContextBase(ContextBase parent, Container container)
        {
            _featuresCache = new SimpleCache<PositionFeature[]>(CreateFeaturesCache);
            Parent = parent;
            Container = container;
            _internalResult = new Result[StatementList.Count];
        }

        [DumpData(false)]
        internal override RefAlignParam RefAlignParam { get { return Parent.RefAlignParam; } }

        [DumpData(false)]
        internal override Root RootContext { get { return Parent.RootContext; } }
        [DumpData(false)]
        public abstract Ref NaturalRefType { get; }
        [DumpData(false)]
        public abstract IRefInCode ForCode { get; }
        [DumpData(false)]
        internal PositionFeature[] Features { get { return _featuresCache.Value; } }
        [DumpData(false)]
        protected abstract int Position { get; }
        [DumpData(false)]
        internal abstract FullContext Context { get; }
        [DumpData(false)]
        internal List<ICompileSyntax> StatementList { get { return Container.List; } }
        [DumpData(false)]
        internal int IndexSize { get { return Container.IndexSize; } }

        private PositionFeature[] CreateFeaturesCache()
        {
            var result = new List<PositionFeature>();
            for (var i = 0; i < Position; i++)
                result.Add(new PositionFeature(EmptyList, this, i));
            return result.ToArray();
        }

        private EmptyList EmptyList
        {
            get { return Container.EmptyList; } }

        internal abstract ContextAtPosition CreatePosition(int position);

        internal override string DumpShort()
        {
            return "context." + ObjectId + "(" + Container.DumpShort() + ")";
        }

        internal override IStructContext FindStruct() { return this; }

        internal Result AccessResultAsArgFromRef(Category category, int position, RefAlignParam refAlignParam)
        {
            return Type(StatementList[position])
                .PostProcessor
                .AccessResultForStruct(category, refAlignParam,
                    () => AccessAsArgCode(position, refAlignParam),
                    Refs.None);
        }

        private CodeBase AccessAsArgCode(int position, RefAlignParam refAlignParam) { return AccessAsArgCode(position, Position, refAlignParam); }

        private CodeBase AccessAsArgCode(int position, int currentPosition, RefAlignParam refAlignParam)
        {
            var offset = Reni.Size.Zero;
            for(var i = position + 1; i < currentPosition; i++)
                offset += InternalSize(i);

            return CodeBase.CreateArg(refAlignParam.RefSize).CreateRefPlus(refAlignParam, offset);
        }

        protected Size InternalSize(int position)
        {
            return InternalResult(Category.Size, position).Size;
        }

        internal Size InternalSize() { return InternalResult(Category.Size).Size; }

        private Result InternalResult(Category category, int position)
        {
            var result = CreatePosition(position)
                .Result(category | Category.Type, StatementList[position])
                .PostProcessor
                .InternalResultForStruct(category,RefAlignParam);
            if (_internalResult[position] == null)
                _internalResult[position] = new Result();
            _internalResult[position].Update(result);
            return result;
        }

        protected Result InternalResult(Category category) { return InternalResult(category, 0, Position); }

        private Result InternalResult(Category category, int fromPosition, int fromNotPosition)
        {
            var result = Reni.Type.Void.CreateResult(category);
            for (var i = fromPosition; i < fromNotPosition; i++)
            {
                var internalResult = InternalResult(category, i);
                result = result.CreateSequence(internalResult);
            }
            return result;
        }

        sealed internal override SearchResult<IContextFeature> Search(Defineable defineable)
        {
            var containerResult = Container.SearchFromStructContext(defineable);
            var result = containerResult.SearchResultDescriptor.Convert(containerResult.Feature,this);
            if(result.IsSuccessFull)
                return result;
            result = Parent.Search(defineable).SubTrial(Parent, "try parent of struct");
            if (result.IsSuccessFull)
                return result;

            return base.Search(defineable).AlternativeTrial(result);
        }

        internal override Result CreateArgsRefResult(Category category) { return Parent.CreateArgsRefResult(category); }

        Result IStructContext.ObjectResult(ContextBase callContext, Category category, ICompileSyntax @object)
        {
            if(@object == null)
                return NaturalRefType.CreateResult(
                    category | Category.Type,
                    () => CodeBase.CreateContextRef(ForCode),
                    () => Refs.Context(ForCode));
            return callContext
                .ResultAsRef(category | Category.Type, @object)
                .ConvertTo(NaturalRefType)
                .CreateRefPlus(category, ForCode.RefAlignParam, NaturalRefType.UnrefSize);
        }

    }
}