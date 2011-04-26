using System.Linq;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using System;
using HWClassLibrary.Helper;
using HWClassLibrary.TreeStructure;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Struct
{
    [Serializable]
    internal abstract class Context : Reni.Context.Child
    {
        private Size[] _offsetsCache;
        private TypeBase[] _typesCache;
        private readonly SimpleCache<ContextPosition[]> _featuresCache;
        private bool _isFunctionInternalSizeActive;

        [Node]
        internal readonly Container Container;

        [Node, IsDumpEnabled(false)]
        private readonly Result[] _internalResult;

        private readonly Type _type;
        private readonly Reference _referenceType;

        [Node, SmartNode]
        private readonly DictionaryEx<ICompileSyntax, FunctionalFeature> _function;

        [Node, IsDumpEnabled(false)]
        private Result _internalConstructorResult;

        [Node, IsDumpEnabled(false)]
        private Result _constructorResult;

        protected Context(ContextBase parent, Container container)
            : base(parent)
        {
            _featuresCache = new SimpleCache<ContextPosition[]>(CreateFeaturesCache);
            Container = container;
            _internalResult = new Result[StatementList.Count];
            _type = new Type(this);
            _referenceType = _type.Reference(parent.RefAlignParam);
            _function = new DictionaryEx<ICompileSyntax, FunctionalFeature>(body => new FunctionalFeature(this, body));
            _internalConstructorResult = new Result();
            _constructorResult = new Result();
        }

        [IsDumpEnabled(false)]
        internal Type ContextType { get { return _type; } }

        [IsDumpEnabled(false)]
        internal Reference ContextReferenceType { get { return _referenceType; } }

        [IsDumpEnabled(false)]
        internal abstract IReferenceInCode ForCode { get; }

        [IsDumpEnabled(false)]
        internal ContextPosition[] Features
        {
            get
            {
                var contextPositions = _featuresCache.Value;
                AssertValid();
                return contextPositions;
            }
        }

        [IsDumpEnabled(false)]
        protected abstract int Position { get; }

        [IsDumpEnabled(false)]
        internal List<ICompileSyntax> StatementList { get { return Container.List; } }

        [IsDumpEnabled(false)]
        private int IndexSize { get { return Container.IndexSize; } }

        private ContextPosition[] CreateFeaturesCache()
        {
            var result = new List<ContextPosition>();
            for(var i = 0; i < Position; i++)
                result.Add(new ContextPosition(this, i));
            return result.ToArray();
        }

        internal abstract ContextAtPosition CreatePosition(int position);

        internal override string DumpShort() { return "context." + ObjectId + "(" + Container.DumpShort() + ")"; }

        internal TypeBase RawType(int position) { return InternalType(position); }

        private IAccessType AccessType(int position) { return InternalType(position).AccessType(this, position); }

        private TypeBase InternalType(int position) { return InternalResult(Category.Type, position).Type; }

        internal Size InternalSize()
        {
            if(_isFunctionInternalSizeActive)
                Tracer.ThrowAssertionFailed("", () => "");
            _isFunctionInternalSizeActive = true;
            var result = InternalResult(Category.Size).Size;
            _isFunctionInternalSizeActive = false;
            return result;
        }

        private Result InternalResult(Category category, int position)
        {
            //Tracer.ConditionalBreak(Container.ObjectId == 2 && position == 3 && category.HasSize, ()=>"");
            var result = CreatePosition(position)
                .Result(category | Category.Type, StatementList[position]);
            if(_internalResult[position] == null)
                _internalResult[position] = new Result();
            _internalResult[position].Update(result);
            return result;
        }

        private Result InternalResult(Category category) { return InternalResult(category, 0, Position); }

        private Result InternalResult(Category category, int fromPosition, int fromNotPosition)
        {
            var result = TypeBase.VoidResult(category);
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
            {
                searchVisitor.InternalResult =
                    Container
                        .SearchFromStructContext(searchVisitor.Defineable)
                        .CheckedConvert(this);
            }
            Parent.Search(searchVisitor);
            base.Search(searchVisitor);
        }

        internal void AssertValid()
        {
            if(_featuresCache.IsValid)
                Tracer.Assert(_featuresCache.Value.Length == Position);
        }

        [IsDumpEnabled(false)]
        internal TypeBase IndexType { get { return TypeBase.Number(IndexSize); } }

        internal Size Offset(int position) { return InternalResult(Category.Size, position + 1, Position).Size; }

        [IsDumpEnabled(false)]
        internal IEnumerable<TypeBase> Types { get { return _typesCache ?? (_typesCache = GetTypes().ToArray()); } }

        [IsDumpEnabled(false)]
        private Size[] Offsets { get { return _offsetsCache ?? (_offsetsCache = GetOffsets().ToArray()); } }

        private IEnumerable<Size> GetOffsets()
        {
            var sizes = Types.Select(typeBase => typeBase.Size).ToArray();
            return sizes.Aggregate(new Size[0], AggregateSizes);
        }

        private Size[] AggregateSizes(Size[] sizesSoFar, Size nextSize)
        {
            var result = sizesSoFar
                .Select(size => size + nextSize.Align(AlignBits))
                .Concat(new[] {Size.Zero})
                .ToArray();
            return result;
        }

        private IEnumerable<TypeBase> GetTypes() { return StatementList.Select(Type); }

        internal override sealed Context FindStruct() { return this; }

        internal Result ThisReferenceResult(Category category) { return ContextReferenceType.Result(category, CreateContextCode, CreateContextRefs); }

        internal Result FunctionalResult(Category category, ICompileSyntax body) { return new FunctionDefinitionType(Function(body)).Result(category); }

        internal Result AccessResultFromArg(Category category, int position)
        {
            return AccessResult(category, position)
                .ReplaceContextReferenceByArg(this);
        }

        internal Result ContextReferenceAsArg(Category category)
        {
            return ContextReferenceType
                .ArgResult(category)
                .AddToReference(category, RefAlignParam, InternalSize(), "ContextReferenceAsArg");
        }

        private Result AccessResult(Category category, int position)
        {
            var accessType = AccessType(position);
            var thisResult = ThisReferenceResult(category | Category.Type);
            var accessResult = accessType.Result(category);
            return accessResult.ReplaceArg(thisResult);
        }

        private CodeBase CreateContextCode()
        {
            return CodeBase
                .ReferenceInCode(ForCode)
                .AddToReference(RefAlignParam, InternalSize()*-1, "CreateContextCode");
        }

        private Refs CreateContextRefs() { return Refs.Create(ForCode); }

        private FunctionalFeature Function(ICompileSyntax body) { return _function.Find(body); }

        internal Result[] CreateArgCodes(Category category)
        {
            return Types
                .Select((type, i) => AutomaticDereference(type, Offsets[i], category))
                .ToArray();
        }

        private Result AutomaticDereference(TypeBase type, Size offset, Category category)
        {
            var reference = type.Reference(RefAlignParam);
            var result = reference
                .Result(category, () => CreateRefArgCode().AddToReference(RefAlignParam, offset, "AutomaticDereference"));
            return result.AutomaticDereference() & category;
        }

        private CodeBase CreateRefArgCode() { return ContextType.Reference(RefAlignParam).ArgCode(); }

        internal Result ConstructorResult(Category category)
        {
            var trace = ObjectId == 3 && category.HasCode;
            StartMethodDumpWithBreak(trace, category);
            var internalResult = InternalResult(category - Category.Type);
            _internalConstructorResult.Update(internalResult);
            var rawResult = ContextType.Result(category, internalResult);
            var result = rawResult.ReplaceRelative(ForCode, () => CodeBase.TopRef(RefAlignParam, "Context.ConstructorResult"));
            Dump(trace, "rawResult", rawResult, "result.Code", result.Code);
            _constructorResult.Update(result);
            return ReturnMethodDumpWithBreak(trace, result);
        }

        internal Refs ConstructorRefs() { return ConstructorResult(Category.Refs).Refs; }

        internal Result PositionFeatureApply(Category category, int position, bool isProperty, bool isContextFeature)
        {
            var trace = position == -1 && category.HasCode && isContextFeature;
            StartMethodDump(trace, category, position, isProperty, isContextFeature);
            var accessResult = AccessResult(isProperty, position, category);
            var result = accessResult;
            if(!isContextFeature)
                result = result.ReplaceContextReferenceByArg(this);
            Dump(trace, "accessCode", accessResult.Code, "result.Code", result.Code);
            return ReturnMethodDumpWithBreak(trace, result);
        }

        private Result AccessResult(bool isProperty, int position, Category category)
        {
            if(isProperty)
            {
                return AccessResult(Category.Type, position)
                    .Type
                    .Apply(category, TypeBase.VoidResult, RefAlignParam);
            }
            return AccessResult(category, position);
        }
    }
}