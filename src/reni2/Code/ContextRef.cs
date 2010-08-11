using HWClassLibrary.TreeStructure;
using System;
using HWClassLibrary.Helper;

namespace Reni.Code
{
    [Serializable]
    internal class ContextRef : LeafElement
    {
        [Node]
        internal readonly IReferenceInCode Context;

        public ContextRef(IReferenceInCode context) { Context = context; }
        protected override Size GetSize() { return Context.RefAlignParam.RefSize; }
        protected override Size GetInputSize() { return Size.Zero; }
        internal Refs GetRefs() { return Refs.Create(Context); }
    }
}