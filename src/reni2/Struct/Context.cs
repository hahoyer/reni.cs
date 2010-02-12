using HWClassLibrary.TreeStructure;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using System;
using HWClassLibrary.Helper;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Syntax;
using Reni.Type;

#pragma warning disable 1911

namespace Reni.Struct
{
    [Serializable]
    internal abstract class Context : ContextBase, IStructContext
    {
        private readonly SimpleCache<ContextPosition[]> _featuresCache;
        [Node]
        internal readonly ContextBase Parent;
        [Node]
        internal readonly Container Container;
        [Node, DumpData(false)]
        private readonly Result[] _internalResult;
        
        protected Context(ContextBase parent, Container container)
        {
            _featuresCache = new SimpleCache<ContextPosition[]>(CreateFeaturesCache);
            Parent = parent;
            Container = container;
            _internalResult = new Result[StatementList.Count];
        }

        [DumpData(false)]
        internal override RefAlignParam RefAlignParam { get { return Parent.RefAlignParam; } }

        [DumpData(false)]
        internal override Root RootContext { get { return Parent.RootContext; } }
        [DumpData(false)]
        public abstract IRefInCode ForCode { get; }

        [DumpData(false)]
        internal ContextPosition[] Features { get { return _featuresCache.Value; } }
        [DumpData(false)]
        protected abstract int Position { get; }

        [DumpData(false)]
        internal List<ICompileSyntax> StatementList { get { return Container.List; } }
        [DumpData(false)]
        internal int IndexSize { get { return Container.IndexSize; } }

        [DumpData(false)]
        internal abstract ThisType ThisType { get; }

        internal bool IsValidRefTarget()
        {
            for(var i = 0; i <= Position; i++)
                if(InternalType(i).IsValidRefTarget())
                    return true;
            return false;
        }

        private ContextPosition[] CreateFeaturesCache()
        {
            var result = new List<ContextPosition>();
            for (var i = 0; i < Position; i++)
                result.Add(new ContextPosition(this, i));
            return result.ToArray();
        }

        internal abstract ContextAtPosition CreatePosition(int position);

        internal override string DumpShort()
        {
            return "context." + ObjectId + "(" + Container.DumpShort() + ")";
        }

        internal override IStructContext FindStruct() { return this; }

        private Size RegionSize(int position, int currentPosition)
        {
            var result = Size.Zero;
            for (var i = position; i < currentPosition; i++)
                result += InternalSize(i);
            return result;
        }

        internal CodeBase ContextRefCodeAsArgCode()
        {
            return CodeBase
                .CreateArg(RefAlignParam.RefSize)
                .CreateRefPlus(RefAlignParam, InternalSize());
        }

        private Size InternalSize(int position)
        {
            return InternalResult(Category.Size, position).Size;
        }

        private TypeBase InternalType(int position)
        {
            return InternalResult(Category.Type, position).Type;
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

        internal override void Search(SearchVisitor<IContextFeature> searchVisitor)
        {
            if(!searchVisitor.IsSuccessFull)
                searchVisitor.InternalResult = 
                    Container
                    .SearchFromStructContext(searchVisitor.Defineable)
                    .CheckedConvert(this);
            Parent.Search(searchVisitor);
            base.Search(searchVisitor);
        }

        internal override Result CreateArgsRefResult(Category category) { return Parent.CreateArgsRefResult(category); }

        internal TypeBase IndexType { get { return TypeBase.CreateNumber(IndexSize);} }

        private CodeBase CreateContextCode()
        {
            return CodeBase
                .CreateContextRef(ForCode)
                .CreateRefPlus(RefAlignParam,InternalSize()*-1);
        }

        private Refs CreateContextRefs()
        {
            return Refs.Context(ForCode);
        }

        Result IStructContext.CreateThisResult(Category category)
        {
            return ThisType.CreateResult(category,CreateContextCode,CreateContextRefs);
        }
    }
}