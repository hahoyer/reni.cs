using HWClassLibrary.TreeStructure;
using System;
using HWClassLibrary.Helper;

namespace Reni.Code
{
    [Serializable]
    internal sealed class ContextRef : FiberItem
    {
        [Node]
        internal readonly IReferenceInCode Context;

        public ContextRef(IReferenceInCode context) { Context = context; }

        internal Refs GetRefs() { return Refs.Create(Context); }
        internal override Size InputSize { get { return Size.Zero; } }
        internal override Size OutputSize { get { return Context.RefAlignParam.RefSize; } }
        protected override void Execute(IFormalMaschine formalMaschine) { throw new NotImplementedException(); }
    }
}