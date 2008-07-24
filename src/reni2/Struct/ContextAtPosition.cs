using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Code;
using Reni.Context;
using Reni.Type;

namespace Reni.Struct
{
    internal sealed class ContextAtPosition : ContextBase
    {
        [Node]
        internal readonly Context Context;
        [Node]
        internal readonly int Position;
        private readonly SimpleCache<TypeAtPosition> _typeCache = new SimpleCache<TypeAtPosition>();


        internal ContextAtPosition(Context context, int position) 
        {
            Context = context;
            Position = position;
        }

        internal override RefAlignParam RefAlignParam { get { return Context.RefAlignParam; } }
        internal override Root RootContext { get { return Context.RootContext; } }
        internal Container Container { get { return Context.Container; } }
        [Node]
        internal ContextBase Parent { get { return Context.Parent; } }
        [Node]
        internal int IndexSize { get { return Context.IndexSize; } }

        internal override Result CreateThisRefResult(Category category)
        {
            return CreateType().CreateContextRefResult(category,this);
        }

        internal TypeAtPosition CreateType()
        {
            return _typeCache.Find(() => new TypeAtPosition(this));
        }

        internal override string DumpShort()
        {
            return Context.DumpShort() + "@" + Position;
        }

        internal Result InternalResult(Category category)
        {
            return Context.InternalResult(category, 0, Position);
        }

        internal Result AccessResultFromRef(Category category, int position, RefAlignParam refAlignParam)
        {
            return Context.AccessResultFromRef(category, position, Position, refAlignParam);
        }
    }

    internal sealed class TypeAtPosition : TypeBase
    {
        [Node]
        internal readonly ContextAtPosition Context;

        public TypeAtPosition(ContextAtPosition context)
        {
            Context = context;
        }

        protected override Size GetSize()
        {
            return Context.InternalResult(Category.Size).Size;
        }

        internal override string DumpShort()
        {
            return "type." + ObjectId + "(context." + Context.DumpShort() + ")";
        }

        internal protected override int IndexSize { get { return Context.IndexSize; } }

        internal override Result AccessResultFromRef(Category category, int position, RefAlignParam refAlignParam)
        {
            return Context.AccessResultFromRef(category, position, refAlignParam);
        }
    }
}