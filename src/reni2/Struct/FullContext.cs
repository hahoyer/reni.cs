using System.Collections.Generic;
using System.Linq;
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
        private TypeBase[] _typesCache;
        private Size[] _offsetsCache;
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

        protected override int Position { get { return StatementList.Count; } }

        internal override ThisType ThisType
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }

        internal override ContextAtPosition CreatePosition(int position)
        {
            return _contextAtPositionCache.Find(position, () => new ContextAtPosition(Context, position));
        }

        [DumpData(false)]
        internal TypeBase[] Types
        {
            get
            {
                if (_typesCache == null)
                    _typesCache = GetTypes().ToArray();
                return _typesCache;
            }
        }

        [DumpData(false)]
        internal Size[] Offsets
        {
            get
            {
                if (_offsetsCache == null)
                    _offsetsCache = GetOffsets().ToArray();
                return _offsetsCache;
            }
        }

        internal Result ConstructorResult(Category category)
        {
            var internalResult = InternalResult(category - Category.Type);
            _internalConstructorResult.Update(internalResult);
            var result = NaturalType.CreateResult(category, internalResult);
            var constructorResult = result
                .ReplaceRelativeContextRef(this, ()=>CodeBase.CreateTopRef(RefAlignParam));
            _constructorResult.Update(constructorResult);
            return constructorResult;
        }

        internal Refs ConstructorRefs() { return ConstructorResult(Category.Refs).Refs; }

        [DumpData(false)]
        private FullContext Context { get { return this; } }
        private TypeBase NaturalType { get { return _typeCache.Value; } }

        private IEnumerable<TypeBase> GetTypes() { return StatementList.Select(syntax => Type(syntax)); }

        private Size[] AggregateSizes(Size[] sizesSoFar, Size nextSize)
        {
            return sizesSoFar.Select(size => size + nextSize.Align(AlignBits)).Union(new[] {Size.Zero}).ToArray();
        }

        private IEnumerable<Size> GetOffsets()
        {
            var sizes = Types.Select(typeBase => typeBase.Size).ToArray();
            return sizes.Aggregate(new Size[0], AggregateSizes);
        }
    }
}