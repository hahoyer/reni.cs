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
    internal abstract class Context : Reni.Context.Child, IRefInCode
    {
        private Size[] _offsetsCache;
        private TypeBase[] _typesCache;
        private readonly SimpleCache<ContextPosition[]> _featuresCache;
        [Node]
        internal readonly Container Container;
        [Node, DumpData(false)]
        private readonly Result[] _internalResult;
        private readonly Type _type;
        private readonly Reni.Type.Reference _referenceType;
        [Node, SmartNode]
        private readonly DictionaryEx<ICompileSyntax, Reni.Type.Function> _function;

        [Node, DumpData(false)]
        private Result _internalConstructorResult;

        [Node, DumpData(false)]
        private Result _constructorResult;

        protected Context(ContextBase parent, Container container)
            : base(parent)
        {
            _featuresCache = new SimpleCache<ContextPosition[]>(CreateFeaturesCache);
            Container = container;
            _internalResult = new Result[StatementList.Count];
            _type = new Type(this);
            _referenceType = _type.CreateReference(parent.RefAlignParam);
            _function = new DictionaryEx<ICompileSyntax, Reni.Type.Function>(body => new Reni.Type.Function(this, body));
            _internalConstructorResult = new Result();
            _constructorResult = new Result();
        }

        RefAlignParam IRefInCode.RefAlignParam { get { return RefAlignParam; } }
        bool IRefInCode.IsChildOf(ContextBase contextBase) { return IsChildOf(contextBase); }

        [DumpData(false)]
        private Type ContextType { get { return _type; } }
        [DumpData(false)]
        internal Reni.Type.Reference ContextReferenceType { get { return _referenceType; } }

        [DumpData(false)]
        protected abstract IRefInCode ForCode { get; }

        [DumpData(false)]
        internal ContextPosition[] Features { get
        {
            ContextPosition[] contextPositions = _featuresCache.Value;
            AssertValid();
            return contextPositions;
        }
        }
        [DumpData(false)]
        protected abstract int Position { get; }

        [DumpData(false)]
        internal List<ICompileSyntax> StatementList { get { return Container.List; } }
        [DumpData(false)]
        private int IndexSize { get { return Container.IndexSize; } }

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

        internal TypeBase RawType(int position) { return InternalType(position); }
        private TypeBase AccessType(int position) { return InternalType(position).AccessType(this, position); }

        private TypeBase InternalType(int position) { return InternalResult(Category.Type, position).Type; }
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

        private Result InternalResult(Category category) { return InternalResult(category, 0, Position); }

        private Result InternalResult(Category category, int fromPosition, int fromNotPosition)
        {
            var result = TypeBase.CreateVoidResult(category);
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
            AssertValid();
            if(!searchVisitor.IsSuccessFull)
                searchVisitor.InternalResult = 
                    Container
                    .SearchFromStructContext(searchVisitor.Defineable)
                    .CheckedConvert(this);
            Parent.Search(searchVisitor);
            base.Search(searchVisitor);
        }

        internal void AssertValid() { 
            if(_featuresCache.IsValid)
                Tracer.Assert(_featuresCache.Value.Length == Position);
        }

        internal override Result CreateArgsReferenceResult(Category category) { return Parent.CreateArgsReferenceResult(category); }

        [DumpData(false)]
        internal TypeBase IndexType { get { return TypeBase.CreateNumber(IndexSize); } }

        internal Size Offset(int position)
        {
            return InternalResult(Category.Size, position+1, Position).Size;
        }

        [DumpData(false)]
        internal IEnumerable<TypeBase> Types { get { return _typesCache ?? (_typesCache = GetTypes().ToArray()); } }
        [DumpData(false)]
        private Size[] Offsets { get
        {
            return _offsetsCache ?? (_offsetsCache = GetOffsets().ToArray());
        } }

        private IEnumerable<Size> GetOffsets()
        {
            var sizes = Types.Select(typeBase => typeBase.Size).ToArray();
            return sizes.Aggregate(new Size[0], AggregateSizes);
        }
        
        private Size[] AggregateSizes(Size[] sizesSoFar, Size nextSize)
        {
            var result = sizesSoFar
                .Select(size => size + nextSize.Align(AlignBits))
                .Concat(new[] { Size.Zero })
                .ToArray();
            return result;
        }

        private IEnumerable<TypeBase> GetTypes() { return StatementList.Select(Type); }

        sealed internal override Context FindStruct() { return this; }

        internal Result CreateThisResult(Category category)
        {
            return ContextReferenceType
                .CreateResult(category, CreateContextCode, CreateContextRefs);
        }

        internal Result CreateContextReference(Category category)
        {
            return ContextReferenceType.CreateArgResult(category);
        }

        internal Result CreateFunctionResult(Category category, ICompileSyntax body)
        {
            return new FunctionDefinitionType(Function(body)).CreateResult(category);
        }

        internal Result CreateAtResultFromArg(Category category, int position)
        {
            return AccessType(position).CreateArgResult(category);
        }

        internal Result CreateAtResultFromContext(Category category, int position)
        {
            return AccessType(position).CreateResult(category, CreateContextCode, CreateContextRefs);
        }

        private CodeBase CreateContextCode()
        {
            return CodeBase
                .Create(ForCode)
                .CreateRefPlus(RefAlignParam, InternalSize() * -1, "CreateContextCode");
        }

        private Refs CreateContextRefs()
        {
            return Refs.Create(ForCode);
        }

        private Reni.Type.Function Function(ICompileSyntax body) { return _function.Find(body); }

        internal Result[] CreateArgCodes(Category category)
        {
            return Types
                .Select((type, i) => AutomaticDereference(type, Offsets[i], category))
                .ToArray();
        }
        private Result AutomaticDereference(TypeBase type, Size offset, Category category)
        {
            return type
                .CreateReference(RefAlignParam)
                .CreateResult(category, () => CreateRefArgCode().CreateRefPlus(RefAlignParam, offset, "AutomaticDereference"))
                .AutomaticDereference()
                & category;
        }

        private CodeBase CreateRefArgCode() { return ContextType.CreateReference(RefAlignParam).CreateArgCode(); }

        internal Result ConstructorResult(Category category)
        {
            var internalResult = InternalResult(category - Category.Type);
            _internalConstructorResult.Update(internalResult);
            var result = ContextType.CreateResult(category, internalResult);
            var constructorResult = result.ReplaceRelative(this, ()=>CodeBase.CreateTopRef(RefAlignParam));
            _constructorResult.Update(constructorResult);
            return constructorResult;
        }

        internal Refs ConstructorRefs() { return ConstructorResult(Category.Refs).Refs; }
    }
}                                    