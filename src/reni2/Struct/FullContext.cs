using HWClassLibrary.TreeStructure;
using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Code;
using Reni.Context;
using Reni.Type;

namespace Reni.Struct
{
    [Serializable]
    internal sealed class FullContext : Context, IRefInCode
    {
        [Node]
        private readonly Result _internalConstructorResult;
        [Node]
        private readonly Result _constructorResult;
        private readonly DictionaryEx<int, ContextAtPosition> _contextAtPositionCache;
        private readonly SimpleCache<Type> _typeCache;

        internal FullContext(ContextBase contextBase, Container container)
            : base(contextBase, container)
        {
            _internalConstructorResult = new Result();
            _contextAtPositionCache = new DictionaryEx<int, ContextAtPosition>();
            _constructorResult = new Result();
            _typeCache = new SimpleCache<Type>(() => new Type(this));
        }

        RefAlignParam IRefInCode.RefAlignParam { get { return RefAlignParam; } }
        bool IRefInCode.IsChildOf(ContextBase contextBase) { return IsChildOf(contextBase); }
        [DumpData(false)]
        public override IRefInCode ForCode { get { return this; } }
        [DumpData(false)]
        private FullContext Context { get { return this; } }

        protected override int Position { get { return StatementList.Count; } }

        private TypeBase NaturalType { get { return _typeCache.Value; } }

        internal override ThisType ThisType
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }

        internal Result ConstructorResult(Category category)
        {
            var internalResult = InternalResult(category - Category.Type);
            _internalConstructorResult.Update(internalResult);
            var constructorResult = NaturalType
                .CreateResult(category, internalResult)
                .ReplaceRelativeContextRef(this, CodeBase.CreateTopRef(RefAlignParam));
            _constructorResult.Update(constructorResult);
            return constructorResult;
        }

        internal override ContextAtPosition CreatePosition(int position)
        {
            return _contextAtPositionCache.Find(position, () => new ContextAtPosition(Context, position));
        }

        internal Refs ConstructorRefs() { return ConstructorResult(Category.Refs).Refs; }

    }
}