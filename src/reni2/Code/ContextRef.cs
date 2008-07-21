using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Context;

namespace Reni.Code
{
    /// <summary>
    /// ContextAtPosition reference, should be replaced
    /// </summary>
    internal sealed class ContextRef<C> : CodeBase where C : ContextBase
    {
        private readonly C _context;

        internal ContextRef(C context)
        {
            _context = context;
            StopByObjectId(-589);
        }

        [Node]
        internal C Context { get { return _context; } }
        internal protected override Size GetSize() { return Context.RefAlignParam.RefSize; } 

        internal override Refs GetRefs()
        {
            return Reni.Refs.Context(_context);
        }

        public override Result VirtVisit<Result>(Visitor<Result> actual)
        {
            return actual.ContextRef(this);
        }
    }
}