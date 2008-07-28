using System;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Parser.TokenClass;
using Reni.Type;

namespace Reni.Struct
{
    internal sealed class ContextAtPosition : ContextBase, IStructContext
    {
        [Node]
        internal readonly Context Context;
        [Node]
        internal readonly int Position;
        private readonly SimpleCache<TypeAtPosition> _typeCache = new SimpleCache<TypeAtPosition>();
        private readonly SimpleCache<PositionFeature[]> _featuresCache = new SimpleCache<PositionFeature[]>();
                                                                         
        internal ContextAtPosition(Context context, int position) 
        {
            Context = context;
            Position = position;
        }

        [DumpData(false)]
        Ref IStructContext.NaturalRefType { get { return NaturalType.CreateRef(RefAlignParam); } }
        [DumpData(false)]
        internal override RefAlignParam RefAlignParam { get { return Context.RefAlignParam; } }
        [DumpData(false)]
        internal override Root RootContext { get { return Context.RootContext; } }
        [DumpData(false)]
        internal Container Container { get { return Context.Container; } }
        [Node]
        internal ContextBase Parent { get { return Context.Parent; } }
        [Node]
        internal int IndexSize { get { return Context.IndexSize; } }
        [DumpData(false)]
        internal PositionFeature[] Features { get { return _featuresCache.Find(CreateFeaturesCache); } }
        [DumpData(false)]
        public TypeBase NaturalType { get { return _typeCache.Find(() => new TypeAtPosition(this)); } }
        [DumpData(false)]
        IContextRefInCode IStructContext.ForCode { get { return Context; } }

        private PositionFeature[] CreateFeaturesCache()
        {
            var result = new List<PositionFeature>();
            for (var i = 0; i < Position; i++)
                result.Add(new PositionFeature(this, i));
            return result.ToArray();
        }

        internal override string DumpShort()
        {
            return Context.DumpShort() + "@" + Position;
        }

        internal override IStructContext FindStruct() { return this; }

        internal Result InternalResult(Category category)
        {
            return Context.InternalResult(category, 0, Position);
        }

        internal Size InternalSize()
        {
            return InternalResult(Category.Size).Size;
        }

        internal Result AccessResultFromRef(Category category, int position, RefAlignParam refAlignParam)
        {
            return Context.AccessResultFromRef(category, position, Position, refAlignParam);
        }

        internal override SearchResult<IContextFeature> Search(Defineable defineable)
        {
            var containerResult = Container.SearchFromStructContextAtPosition(defineable);
            var result = containerResult.SearchResultDescriptor.Convert(containerResult.Feature, this);
            if (result.IsSuccessFull)
                return result;
            return base.Search(defineable).AlternativeTrial(result);
        }

    }

    internal sealed class TypeAtPosition : TypeBase
    {
        [Node]
        internal readonly ContextAtPosition Context;

        public TypeAtPosition(ContextAtPosition context)
        {
            Context = context;
        }

        protected override Size GetSize()
        {
            return Context.InternalResult(Category.Size).Size;
        }

        internal override string DumpShort()
        {
            return "type." + ObjectId + "(context." + Context.DumpShort() + ")";
        }

        internal protected override int IndexSize { get { return Context.IndexSize; } }

        internal override Result AccessResultFromRef(Category category, int position, RefAlignParam refAlignParam)
        {
            return Context.AccessResultFromRef(category, position, refAlignParam);
        }
    }
}