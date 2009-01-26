using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Code;
using Reni.Context;
using Reni.Type;

namespace Reni.Struct
{
    [Serializable]
    internal sealed class FullContext : StructContextBase, IRefInCode
    {
        [Node]
        private readonly Result _internalConstructorResult = new Result();
        [Node]
        private readonly Result _constructorResult = new Result();
        private readonly DictionaryEx<int, ContextAtPosition> _contextAtPositionCache = new DictionaryEx<int, ContextAtPosition>();
        private readonly SimpleCache<Type> _typeCache = new SimpleCache<Type>();

        internal FullContext(ContextBase contextBase, Container container)
            : base(contextBase, container) { }

        RefAlignParam IRefInCode.RefAlignParam { get { return RefAlignParam; } }
        bool IRefInCode.IsChildOf(ContextBase contextBase) { return IsChildOf(contextBase); }
        [DumpData(false)]
        public override IRefInCode ForCode { get { return this; } }
        [DumpData(false)]
        internal override FullContext Context { get { return this; } }
        internal override int Position { get { return StatementList.Count; } }

        [DumpData(false)]
        public override Ref NaturalRefType { get { return NaturalType.CreateAutomaticRef(RefAlignParam); } }

        private TypeBase NaturalType { get { return _typeCache.Find(() => new Type(this)); } }

        internal Result ConstructorResult(Category category)
        {
            var internalResult = InternalResult(category - Category.Type);
            _internalConstructorResult.Update(internalResult);
            var constructorResult = NaturalType.CreateResult(category, internalResult)
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