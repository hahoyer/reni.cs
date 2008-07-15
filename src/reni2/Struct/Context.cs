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
    internal sealed class Context : Reni.Context.Child
    {
        private readonly DictionaryEx<int, ContextAtPosition> _contextAtPositionCache = new DictionaryEx<int, ContextAtPosition>();
        private readonly SimpleCache<Type> _naturalTypeCache = new SimpleCache<Type>();
        internal readonly Container Container;
        private readonly SimpleCache<PositionFeature[]> _featuresCache = new SimpleCache<PositionFeature[]>();

        internal Context(ContextBase contextBase, Container container) : base(contextBase)
        {
            Container = container;
        }

        [DumpData(false)]
        internal List<ICompileSyntax> StatementList { get { return Container.List; } }
        [DumpData(false)]
        internal int IndexSize { get { return Container.IndexSize; } }
        [DumpData(false)]
        internal PositionFeature[] Features { get { return _featuresCache.Find(CreateFeaturesCache); } }
        [DumpData(false)]
        internal Type NaturalType { get { return _naturalTypeCache.Find(() => new Type(this)); } }
        [DumpData(false)]
        internal Ref NaturalRefType { get { return NaturalType.CreateRef(RefAlignParam); } }

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
                result = result.CreateSequence(InternalResult(category, i));
            return result;
        }

        private Result InternalResult(Category category, int position)
        {
            return PostProcessStrategy.InternalResult(CreatePosition(position).Result(category | Category.Type, StatementList[position]), AlignBits);
        }

        private Size InternalSize(int position)
        {
            return InternalResult(Category.Size, position).Size;
        }

        internal Result AccessResultFromRef(Category category, int position, int currentPosition, RefAlignParam refAlignParam)
        {
            return PostProcessStrategy
                .AccessResultFromRef(
                category,
                Type(StatementList[position]),
                refAlignParam,
                () => AccessCode(position, currentPosition, refAlignParam));
        }

        private CodeBase AccessCode(int position, int currentPosition, RefAlignParam refAlignParam)
        {
            var offset = Reni.Size.Zero;
            for(var i = position + 1; i < currentPosition; i++)
                offset += InternalSize(i);
            return new Arg(refAlignParam.RefSize).CreateRefPlus(refAlignParam, offset);
        }

        internal override string DumpShort()
        {
            return "context." + ObjectId + "(" + Container.DumpShort() + ")";
        }

        internal override Result CreateThisRefResult(Category category)
        {
            return NaturalType
                .CreateRef(RefAlignParam)
                .CreateResult(category, () => CodeBase.CreateTopRef(RefAlignParam));
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
    }

    internal static class PostProcessStrategy
    {
        public static Result AccessResultFromRef(Category category, TypeBase typeBase, RefAlignParam refAlignParam, Result.GetCode getCode)
        {
            return typeBase.AutomaticDereference().CreateAssignableRefResult(category, refAlignParam, getCode);
        }

        public static Result InternalResult(Result result, int AlignBits)
        {
            return result.AutomaticDereference().Align(AlignBits);
        }
    }
}