using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Code;
using Reni.Context;

namespace Reni.Struct
{
    [Serializable]
    internal sealed class FullContext : StructContextBase, IRefInCode
    {
        [Node]
        private readonly Result _internalConstructorResult = new Result();
        [Node]
        private readonly Result _constructorResult = new Result();

        internal FullContext(ContextBase contextBase, Container container)
            : base(contextBase, container) { }

        RefAlignParam IRefInCode.RefAlignParam { get { return RefAlignParam; } }
        bool IRefInCode.IsChildOf(ContextBase contextBase) { return IsChildOf(contextBase); }
        [DumpData(false)]
        public override IRefInCode ForCode { get { return this; } }
        [DumpData(false)]
        internal override FullContext Context { get { return this; } }
        internal override int Position { get { return StatementList.Count; } }

        internal Result ConstructorResult(Category category)
        {
            var internalResult = InternalResult(category - Category.Type);
            _internalConstructorResult.Update(internalResult);
            var constructorResult = NaturalType.CreateResult(category, internalResult)
                .ReplaceRelativeContextRef(this, CodeBase.CreateTopRef(RefAlignParam));
            _constructorResult.Update(constructorResult);
            return constructorResult;
        }
    }
}