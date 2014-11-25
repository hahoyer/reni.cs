using System;
using System.Collections.Generic;
using System.Linq;
using hw.Forms;
using Reni.Basics;

namespace Reni.Code
{
    /// <summary>
    ///     ContextAtPosition reference, should be replaced
    /// </summary>
    sealed class ReferenceCode : FiberHead
    {
        readonly IContextReference _context;
        static int _nextObjectId;

        internal ReferenceCode(IContextReference context)
            : base(_nextObjectId++)
        {
            _context = context;
            StopByObjectId(-5);
        }

        [Node]
        internal IContextReference Context { get { return _context; } }

        protected override CodeArgs GetRefsImplementation() { return CodeArgs.Create(_context); }

        protected override Size GetSize() { return _context.Size; }
        protected override TResult VisitImplementation<TResult>(Visitor<TResult> actual) { return actual.ContextRef(this); }
        internal override void Visit(IVisitor visitor) { visitor.ReferenceCode(Context); }

        public override string DumpData() { return _context.NodeDump(); }
    }
}