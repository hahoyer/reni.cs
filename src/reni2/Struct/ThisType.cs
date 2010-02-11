using System;
using Reni.Code;
using Reni.Type;

namespace Reni.Struct
{
    internal sealed class ThisType : TypeBase
    {
        private readonly Context _context;

        internal ThisType(Context context) { _context = context; }

        protected override Size GetSize() { return _context.RefSize; }
        protected override ThisType GetThisType() { return this; }
        internal override string DumpShort() { return "type(this)"; }
        internal override bool IsValidRefTarget() { return false; }

        internal Result AccessResult(Category category, int position)
        {
            NotImplementedMethod(category, position);
            return null;
        }

        internal TypeBase IndexType { get { return _context.IndexType; } }

        internal Result CreateContextResult(Category category)
        {
            return CreateResult(
                category,
                () =>
                CodeBase.CreateContextRef(_context.ForCode).CreateRefPlus(_context.RefAlignParam,
                                                                          _context.InternalSize()*-1),
                () => Refs.Context(_context.ForCode));
        }
    }
}