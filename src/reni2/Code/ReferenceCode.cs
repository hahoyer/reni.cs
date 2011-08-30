using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using Reni.Basics;
using Reni.Context;

namespace Reni.Code
{
    /// <summary>
    ///     ContextAtPosition reference, should be replaced
    /// </summary>
    internal sealed class ReferenceCode : FiberHead
    {
        private readonly IReferenceInCode _context;
        private static int _nextObjectId;

        internal ReferenceCode(IReferenceInCode context)
            : base(_nextObjectId++)
        {
            _context = context;
            StopByObjectId(4);
        }

        [Node]
        internal IReferenceInCode Context { get { return _context; } }

        [DisableDump]
        internal override RefAlignParam RefAlignParam { get { return _context.RefAlignParam; } }
        [DisableDump]
        internal override bool IsRelativeReference { get { return false; } }

        protected override CodeArgs GetRefsImplementation() { return CodeArgs.Create(_context); }

        protected override Size GetSize() { return RefAlignParam.RefSize; }
        protected override TResult VisitImplementation<TResult>(Visitor<TResult> actual) { return actual.ContextRef(this); }
        protected override void Execute(IFormalMaschine formalMaschine) { formalMaschine.ReferenceCode(Context); }
    }
}