using System;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using Reni.Context;

namespace Reni.Code
{
    /// <summary>
    /// ContextAtPosition reference, should be replaced
    /// </summary>
    internal sealed class ReferenceCode : FiberHead
    {
        readonly IReferenceInCode _context;
        private static int _nextObjectId;

        internal ReferenceCode(IReferenceInCode context):base(_nextObjectId++)
        {
            _context = context;
            StopByObjectId(-2);
        }

        internal IReferenceInCode Context { get { return _context; } }

        [IsDumpEnabled(false)]
        internal override RefAlignParam RefAlignParam { get { return _context.RefAlignParam; } }

        protected override Refs GetRefsImplementation() { return Refs.Create(_context); }

        protected override Size GetSize() { return RefAlignParam.RefSize; }
        protected override TResult VisitImplementation<TResult>(Visitor<TResult> actual) { return actual.ContextRef(this); }
        protected override void Execute(IFormalMaschine formalMaschine) { formalMaschine.ReferenceCode(Context); }
    }
}