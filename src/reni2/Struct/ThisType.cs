using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
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
        internal Result AccessResult(Category category, int position) { return At(position).CreateArgResult(category); }
        internal TypeBase IndexType { get { return _context.IndexType; } }
        internal StructRef At(int position) { return new StructRef(_context, position); }
    }

}