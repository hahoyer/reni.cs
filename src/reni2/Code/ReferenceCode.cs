using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Forms;
using Reni.Basics;
using Reni.Struct;
using Reni.Type;

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
            var compoundView = ((context as PointerType)
                ?.ValueType as CompoundType)
                ?.View;
            Tracer.ConditionalBreak
                (compoundView?.Compound.Syntax.ObjectId == -11 && compoundView.ViewPosition == 4);
            StopByObjectIds();
        }

        [Node]
        internal IContextReference Context => _context;

        protected override CodeArgs GetRefsImplementation() => CodeArgs.Create(_context);

        protected override Size GetSize() => _context.Size;

        protected override TCode VisitImplementation<TCode, TFiber>(Visitor<TCode, TFiber> actual)
            => actual.ContextRef(this);

        internal override void Visit(IVisitor visitor)
        {
            throw new UnexpectedContextReference(_context);
        }

        public override string DumpData() => _context.NodeDump();
    }
}