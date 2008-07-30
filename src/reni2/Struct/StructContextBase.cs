using System.Collections.Generic;
using HWClassLibrary.Debug;
using System;
using HWClassLibrary.Helper;
using Reni.Code;
using Reni.Context;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Struct
{
    internal abstract class StructContextBase : ContextBase, IStructContext
    {
        private readonly SimpleCache<Type> _typeCache = new SimpleCache<Type>();
        private readonly SimpleCache<PositionFeature[]> _featuresCache = new SimpleCache<PositionFeature[]>();
        private readonly DictionaryEx<int, ContextAtPosition> _contextAtPositionCache = new DictionaryEx<int, ContextAtPosition>();
        [Node]
        internal readonly ContextBase Parent;
        [Node, DumpData(false)]
        internal readonly Container Container;
        [Node, DumpData(false)]
        internal readonly Result[] _internalResult;
        
        protected StructContextBase(ContextBase parent, Container container)
        {
            Parent = parent;
            Container = container;
            _internalResult = new Result[StatementList.Count];
        }

        [DumpData(false)]
        internal override RefAlignParam RefAlignParam { get { return Parent.RefAlignParam; } }
        [DumpData(false)]
        internal override Root RootContext { get { return Parent.RootContext; } }
        [DumpData(false)]
        Ref IStructContext.NaturalRefType { get { return NaturalType.CreateAutomaticRef(RefAlignParam); } }
        [DumpData(false)]
        public TypeBase NaturalType { get { return _typeCache.Find(() => new Type(this)); } }
        [DumpData(false)]
        public abstract IContextRefInCode ForCode { get; }
        [DumpData(false)]
        internal PositionFeature[] Features { get { return _featuresCache.Find(CreateFeaturesCache); } }
        [DumpData(false)]
        internal abstract int Position { get; }
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
                result.Add(new PositionFeature(this, i));
            return result.ToArray();
        }

        internal ContextAtPosition CreatePosition(int position)
        {
            return _contextAtPositionCache.Find(position, () => new ContextAtPosition(Context, position));
        }

        internal override string DumpShort()
        {
            return "context." + ObjectId + "(" + Container.DumpShort() + ")";
        }

        internal override IStructContext FindStruct() { return this; }

        internal Result AccessResultFromRef(Category category, int position, RefAlignParam refAlignParam)
        {
            return Type(StatementList[position])
                .PostProcessor
                .AccessResultForStruct(category, refAlignParam,
                    () => AccessCode(position, refAlignParam));
        }

        private CodeBase AccessCode(int position, RefAlignParam refAlignParam) { return AccessCode(position, Position, refAlignParam); }

        private CodeBase AccessCode(int position, int currentPosition, RefAlignParam refAlignParam)
        {
            var offset = Reni.Size.Zero;
            for (var i = position + 1; i < currentPosition; i++)
            {
                var internalSize = InternalSize(i);
                if (internalSize.IsPending)
                    return CodeBase.Pending;
                offset += internalSize;
            }

            return new Arg(refAlignParam.RefSize).CreateRefPlus(refAlignParam, offset);
        }

        private Size InternalSize(int position)
        {
            return InternalResult(Category.Size, position).Size;
        }

        internal Size InternalSize() { return InternalResult(Category.Size).Size; }

        private Result InternalResult(Category category, int position)
        {
            Result result = CreatePosition(position)
                .Result(category | Category.Type, StatementList[position])
                .PostProcessor
                .InternalResultForStruct(AlignBits);
            if (_internalResult[position] == null)
                _internalResult[position] = new Result();
            _internalResult[position].Update(result);
            return result;
        }

        internal Result InternalResult(Category category) { return InternalResult(category, 0, Position); }

        internal Result InternalResult(Category category, int fromPosition, int fromNotPosition)
        {
            var result = Reni.Type.Void.CreateResult(category);
            for (var i = fromPosition; i < fromNotPosition; i++)
            {
                var internalResult = InternalResult(category, i);
                result = result.CreateSequence(internalResult);
            }
            return result;
        }


    }
}