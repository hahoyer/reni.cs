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
        public abstract Ref NaturalRefType { get; }
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

        internal Result AccessResultAsArgFromRef(Category category, int position, RefAlignParam refAlignParam)
        {
            return Type(StatementList[position])
                .PostProcessor
                .AccessResultForStruct(category, refAlignParam,
                    () => AccessAsArgCode(position, refAlignParam),
                    Refs.None);
        }

        internal Result AccessResultAsContextRef(Category category, int position)
        {
            return Type(StatementList[position])
                .PostProcessor
                .AccessResultForStruct(category, ForCode.RefAlignParam,
                    () => AccessCodeAsContextRef(position),
                    () => Refs.Context(ForCode));
        }

        private CodeBase AccessAsArgCode(int position, RefAlignParam refAlignParam) { return AccessAsArgCode(position, Position, refAlignParam); }

        private CodeBase AccessCodeAsContextRef(int position)
        {
            var offset = Size.Zero;
            for (var i = 0; i <= position; i++)
                offset += InternalSize(i);

            return CodeBase.CreateContextRef(ForCode).CreateRefPlus(ForCode.RefAlignParam, offset*(-1));
        }

        private CodeBase AccessAsArgCode(int position, int currentPosition, RefAlignParam refAlignParam)
        {
            var offset = Size.Zero;
            for(var i = position + 1; i < currentPosition; i++)
                offset += InternalSize(i);

            return CodeBase.CreateArg(refAlignParam.RefSize).CreateRefPlus(refAlignParam, offset);
        }

        internal CodeBase ContextRefCodeAsArgCode()
        {
            return CodeBase
                .CreateArg(RefAlignParam.RefSize)
                .CreateRefPlus(RefAlignParam, InternalSize());
        }

        protected Size InternalSize(int position)
        {
            return InternalResult(Category.Size, position).Size;
        }

        protected TypeBase InternalType(int position)
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

        Result ObjectResult(ContextBase callContext, Category category, ICompileSyntax @object)
        {
            if(@object == null)
                return CreateContextRef(category);
            return callContext
                .ResultAsRef(category | Category.Type, @object)
                .ConvertTo(NaturalRefType)
                .CreateRefPlus(category, ForCode.RefAlignParam, NaturalRefType.UnrefSize);
        }

        private Result CreateContextRef(Category category)
        {
            return NaturalRefType.CreateResult(
                category | Category.Type,
                () => CodeBase.CreateContextRef(ForCode),
                () => Refs.Context(ForCode));
        }

    }
}