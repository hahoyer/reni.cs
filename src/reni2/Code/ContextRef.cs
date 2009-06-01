using HWClassLibrary.TreeStructure;
using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;

namespace Reni.Code
{
    /// <summary>
    /// ContextAtPosition reference, should be replaced
    /// </summary>
    internal sealed class RefCode : CodeBase
    {
        private readonly ContextRef _leafElement;

        internal RefCode(IRefInCode context)
        {
            _leafElement = new ContextRef(context);
            StopByObjectId(-2766);
        }

        [Node]
        internal IRefInCode Context { get { return _leafElement.Context; } }

        [DumpData(false)]
        internal LeafElement ToLeafElement { get { return _leafElement; } }

        [DumpData(false)]
        protected override Size SizeImplementation { get { return _leafElement.Size; } }
        [DumpData(false)]
        internal override Refs RefsImplementation { get { return _leafElement.GetRefs(); } }
        public override Result VisitImplementation<Result>(Visitor<Result> actual) { return actual.ContextRef(this); }
    }

    [Serializable]
    internal class ContextRef : LeafElement
    {
        [Node]
        internal readonly IRefInCode Context;

        public ContextRef(IRefInCode context) { Context = context; }
        protected override Size GetSize() { return Context.RefSize; }
        protected override Size GetInputSize() { return Size.Zero; }
        internal Refs GetRefs() { return Refs.Context(Context); }
    }
}