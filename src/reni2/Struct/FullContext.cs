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
    internal sealed class FullContext : StructContextBase, IRefInCode
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
        internal override FullContext Context { get { return this; } }

        protected override int Position { get { return StatementList.Count; } }

        [DumpData(false)]
        public override Ref NaturalRefType { get { return NaturalType.CreateAutomaticRef(RefAlignParam); } }

        private TypeBase NaturalType { get { return _typeCache.Value; } }

        internal Result ConstructorResult(Category category)
        {
            var internalResult = InternalResult(category - Category.Type);
            _internalConstructorResult.Update(internalResult);
            var constructorResult = NaturalType.CreateResult(category, internalResult)
                .ReplaceRelativeContextRef(this, CodeBase.CreateTopRef(RefAlignParam));
            _constructorResult.Update(constructorResult);
            return constructorResult;
        }

        private CodeBase AccessAsContextRefCode(int position, RefAlignParam refAlignParam)
        {
            var offset = Reni.Size.Zero;
            for (var i = 0; i <= position; i++)
                offset -= InternalSize(i);

            return CodeBase.CreateContextRef(this).CreateRefPlus(refAlignParam, offset);
        }

        internal Result AccessResultAsContextRefFromRef(Category category, int position, RefAlignParam refAlignParam)
        {
            return Type(StatementList[position])
                .PostProcessor
                .AccessResultForStruct(category, refAlignParam,
                    () => AccessAsContextRefCode(position, refAlignParam),
                    () => Refs.Context(this));
        }

        internal override ContextAtPosition CreatePosition(int position)
        {
            return _contextAtPositionCache.Find(position, () => new ContextAtPosition(Context, position));
        }

        internal Refs ConstructorRefs() { return ConstructorResult(Category.Refs).Refs; }
    }
}