using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.TreeStructure;
using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Code;
using Reni.Context;
using Reni.Syntax;

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

        internal FullContext(ContextBase contextBase, Container container)
            : base(contextBase, container)
        {
            _internalConstructorResult = new Result();
            _contextAtPositionCache = new DictionaryEx<int, ContextAtPosition>(position => new ContextAtPosition(Context, position));
            _constructorResult = new Result();
        }

        RefAlignParam IRefInCode.RefAlignParam { get { return RefAlignParam; } }
        bool IRefInCode.IsChildOf(ContextBase contextBase) { return IsChildOf(contextBase); }

        [DumpData(false)]
        public override IRefInCode ForCode { get { return this; } }

        protected override int Position { get { return StatementList.Count; } }

        internal override ContextAtPosition CreatePosition(int position)
        {
            return _contextAtPositionCache.Find(position);
        }

        internal Result ConstructorResult(Category category)
        {
            var internalResult = InternalResult(category - Category.Type);
            _internalConstructorResult.Update(internalResult);
            var result = ContextType.CreateResult(category, internalResult);
            var constructorResult = result
                .ReplaceRelative(this, ()=>CodeBase.CreateTopRef(RefAlignParam));
            _constructorResult.Update(constructorResult);
            return constructorResult;
        }

        [DumpData(false)]
        private FullContext Context { get { return this; } }

        internal Refs ConstructorRefs() { return ConstructorResult(Category.Refs).Refs; }

    }
}