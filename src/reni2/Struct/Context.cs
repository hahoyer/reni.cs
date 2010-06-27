using System.Linq;
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

namespace Reni.Struct
{
    [Serializable]
    internal abstract class Context : ContextBase, IStructContext
    {
        private Size[] _offsetsCache;
        private TypeBase[] _typesCache;
        private readonly SimpleCache<ContextPosition[]> _featuresCache;
        [Node]
        internal readonly ContextBase Parent;
        [Node]
        internal readonly Container Container;
        [Node, DumpData(false)]
        private readonly Result[] _internalResult;
        private readonly Type _type;
        private readonly Reni.Type.Reference _referenceType;

        protected Context(ContextBase parent, Container container)
        {
            _featuresCache = new SimpleCache<ContextPosition[]>(CreateFeaturesCache);
            Parent = parent;
            Container = container;
            _internalResult = new Result[StatementList.Count];
            _type = new Type(this);
            _referenceType = _type.CreateReference(parent.RefAlignParam);
        }

        [DumpData(false)]
        internal Type ContextType { get { return _type; } }
        [DumpData(false)]
        internal Reni.Type.Reference ContextReferenceType { get { return _referenceType; } }

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

        internal TypeBase InternalType(int position)
        {
            return InternalResult(Category.Type, position).Type;
        }

        internal Size InternalSize() { return InternalResult(Category.Size).Size; }

        private Result InternalResult(Category category, int position)
        {
            var result = CreatePosition(position)
                .Result(category | Category.Type, StatementList[position]);
            if (_internalResult[position] == null)
                _internalResult[position] = new Result();
            _internalResult[position].Update(result);
            return result;
        }

        protected Result InternalResult(Category category) { return InternalResult(category, 0, Position); }

        private Result InternalResult(Category category, int fromPosition, int fromNotPosition)
        {
            var result = Reni.Type.Void.CreateResult(category);
            for(var i = fromPosition; i < fromNotPosition; i++)
                result = result.CreateSequence(InternalAlignedResult(category, i));
            return result;
        }

        private Result InternalAlignedResult(Category category, int i)
        {
            return InternalResult(category, i)
                .PostProcessor
                .InternalResultForStruct(category, RefAlignParam);
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

        internal TypeBase IndexType { get { return TypeBase.CreateNumber(IndexSize); } }

        internal Size Offset(int position)
        {
            return InternalResult(Category.Size, position+1, Position).Size;
        }

        [DumpData(false)]
        internal IEnumerable<TypeBase> Types { get { return _typesCache ?? (_typesCache = GetTypes().ToArray()); } }
        [DumpData(false)]
        internal Size[] Offsets { get { return _offsetsCache ?? (_offsetsCache = GetOffsets().ToArray()); } }

        private IEnumerable<Size> GetOffsets()
        {
            var sizes = Types.Select(typeBase => typeBase.Size).ToArray();
            return sizes.Aggregate(new Size[0], AggregateSizes);
        }
        
        private Size[] AggregateSizes(Size[] sizesSoFar, Size nextSize)
        {
            return sizesSoFar
                .Select(size => size + nextSize.Align(AlignBits))
                .Union(new[] { Size.Zero })
                .ToArray();
        }

        private IEnumerable<TypeBase> GetTypes() { return StatementList.Select(x=>Type(x)); }

        sealed internal override IStructContext FindStruct() { return this; }

        Result IStructContext.CreateThisResult(Category category)
        {
            return ContextReferenceType
                .CreateResult(category, CreateContextCode, CreateContextRefs);
        }

        internal Result CreateAtResultFromArg(Category category, int position)
        {
            return new Reference(this, position)
                .CreateArgResult(category);
        }

        internal Result CreateAtResultFromContext(Category category, int position)
        {
            return new Reference(this, position)
                .CreateResult(category, CreateContextCode, CreateContextRefs);
        }

        private CodeBase CreateContextCode()
        {
            return CodeBase
                .CreateContextRef(ForCode)
                .CreateRefPlus(RefAlignParam, InternalSize() * -1);
        }

        private Refs CreateContextRefs()
        {
            return Refs.Context(ForCode);
        }

    }
}                                    