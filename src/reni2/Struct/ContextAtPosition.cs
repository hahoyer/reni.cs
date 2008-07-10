using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Code;
using Reni.Type;

namespace Reni.Struct
{
    internal sealed class ContextAtPosition : Reni.Context.Child
    {
        [DumpData(false)]
        internal readonly Context Context;
        [DumpData(false)]
        internal readonly int Position;
        private readonly SimpleCache<TypeAtPosition> _typeCache = new SimpleCache<TypeAtPosition>();

        internal ContextAtPosition(Context context, int position) : base(context)
        {
            Context = context;
            Position = position;
        }

        internal int IndexSize { get { return Context.IndexSize; } }

        internal override Result CreateThisRefResult(Category category)
        {
            return CreateType()
                .CreateRef(RefAlignParam)
                .CreateResult(category, () => CodeBase.CreateTopRef(RefAlignParam));
        }

        internal TypeAtPosition CreateType()
        {
            return _typeCache.Find(() => new TypeAtPosition(this));
        }

        internal override string DumpShort()
        {
            return Context.DumpShort() + "@"+Position;
        }

        internal Result InternalResult(Category category)
        {
            return Context.InternalResult(category, 0, Position);
        }
    }

    internal sealed class TypeAtPosition : TypeBase
    {
        internal readonly ContextAtPosition Context;

        public TypeAtPosition(ContextAtPosition context)
        {
            Context = context;
        }

        internal override Size Size { get { return Context.InternalResult(Category.Size).Size;} }

        internal override string DumpShort()
        {
            return "type." + ObjectId + "(context." + Context.DumpShort() + ")";
        }

        internal protected override int IndexSize { get { return Context.IndexSize; } }
    }
}