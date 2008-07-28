using System;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Parser.TokenClass;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Struct
{
    internal sealed class Context : Reni.Context.Child, IStructContext, IContextRefInCode
    {
        private readonly DictionaryEx<int, ContextAtPosition> _contextAtPositionCache = new DictionaryEx<int, ContextAtPosition>();
        private readonly SimpleCache<Type> _naturalTypeCache = new SimpleCache<Type>();
        internal readonly Container Container;
        private readonly SimpleCache<PositionFeature[]> _featuresCache = new SimpleCache<PositionFeature[]>();
        [Node, DumpData(false)]
        internal readonly Result[] _internalResult;
        [Node]
        private readonly Result _internalConstructorResult = new Result();
        [Node]
        private readonly Result _constructorResult = new Result();


        internal Context(ContextBase contextBase, Container container) : base(contextBase)
        {
            Container = container;
            _internalResult = new Result[StatementList.Count];
        }

        bool IContextRefInCode.IsChildOf(ContextBase contextBase) { return IsChildOf(contextBase); }
        [DumpData(false)]
        Ref IStructContext.NaturalRefType { get { return NaturalType.CreateRef(RefAlignParam); } }
        [DumpData(false)]
        internal List<ICompileSyntax> StatementList { get { return Container.List; } }
        [DumpData(false)]
        internal int IndexSize { get { return Container.IndexSize; } }
        [DumpData(false)]
        internal PositionFeature[] Features { get { return _featuresCache.Find(CreateFeaturesCache); } }
        [Node, DumpData(false)]
        public TypeBase NaturalType { get { return _naturalTypeCache.Find(() => new Type(this)); } }
        [DumpData(false)]
        IContextRefInCode IStructContext.ForCode { get { return this; } }

        internal ContextAtPosition CreatePosition(int position)
        {
            return _contextAtPositionCache.Find(position, () => new ContextAtPosition(this, position));
        }

        internal Result InternalResult(Category category)
        {
            return InternalResult(category, 0, StatementList.Count);
        }

        internal Result InternalResult(Category category, int fromPosition, int fromNotPosition)
        {
            var result = Reni.Type.Void.CreateResult(category);
            for(var i = fromPosition; i < fromNotPosition; i++)
            {
                var internalResult = InternalResult(category, i);
                result = result.CreateSequence(internalResult);
            }
            return result;
        }

        private Result InternalResult(Category category, int position)
        {
            Result result = 
                CreatePosition(position)
                .Result(category | Category.Type, StatementList[position])
                .PostProcessor
                .InternalResultForStruct(AlignBits);
            if (_internalResult[position] == null)
                _internalResult[position] = new Result();
            _internalResult[position].Update(result);
            return result;
        }

        private Size InternalSize(int position)
        {
            return InternalResult(Category.Size, position).Size;
        }

        internal Result AccessResultFromRef(Category category, int position, int currentPosition, RefAlignParam refAlignParam)
        {
            return Type(StatementList[position])
                .PostProcessor
                .AccessResultForStruct(category, refAlignParam, () => AccessCode(position, currentPosition, refAlignParam));
        }

        private CodeBase AccessCode(int position, int currentPosition, RefAlignParam refAlignParam)
        {
            var offset = Reni.Size.Zero;
            for(var i = position + 1; i < currentPosition; i++)
            {
                var internalSize = InternalSize(i);
                if(internalSize.IsPending)
                    return CodeBase.Pending;
                offset += internalSize;
            }

            return new Arg(refAlignParam.RefSize).CreateRefPlus(refAlignParam, offset);
        }

        internal override string DumpShort()
        {
            return "context." + ObjectId + "(" + Container.DumpShort() + ")";
        }

        internal Result AccessResultFromRef(Category category, int position, RefAlignParam refAlignParam)
        {
            return AccessResultFromRef(category, position, StatementList.Count, refAlignParam);
        }

        internal override SearchResult<IContextFeature> Search(Defineable defineable)
        {
            var containerResult = Container.SearchFromStructContext(defineable);
            var result = containerResult.SearchResultDescriptor.Convert(containerResult.Feature, this);
            if(result.IsSuccessFull)
                return result;
            return base.Search(defineable).AlternativeTrial(result);
        }

        private PositionFeature[] CreateFeaturesCache()
        {
            var result = new List<PositionFeature>();
            for(var i = 0; i < StatementList.Count; i++)
                result.Add(new PositionFeature(this, i));
            return result.ToArray();
        }

        internal Result ConstructorResult(Category category)
        {
            var internalResult = InternalResult(category - Category.Type);
            _internalConstructorResult.Update(internalResult);
            var constructorResult = NaturalType.CreateResult(category, internalResult)
                .ReplaceRelativeContextRef(this, CodeBase.CreateTopRef(RefAlignParam));
            _constructorResult.Update(constructorResult);
            return constructorResult;
        }

        RefAlignParam IContextRefInCode.RefAlignParam { get { return RefAlignParam; } }
    }
}