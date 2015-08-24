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
            StopByObjectIds();
        }

        [Node]
        internal IContextReference Context => _context;

        protected override CodeArgs GetRefsImplementation() => CodeArgs.Create(_context);

        protected override Size GetSize() => _context.Size;
        protected override TResult VisitImplementation<TResult>(Visitor<TResult> actual)
            => actual.ContextRef(this);
        internal override void Visit(IVisitor visitor) => visitor.ReferenceCode(Context);

        public override string DumpData() => _context.NodeDump();
    }
}