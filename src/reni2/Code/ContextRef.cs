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
            StopByObjectId(-363);
        }

        [Node]
        internal IRefInCode Context { get { return _leafElement.Context; } }

        [DumpData(false)]
        internal LeafElement ToLeafElement { get { return _leafElement; } }

        protected override Size GetSize() { return _leafElement.Size; }
        internal override Refs GetRefs() { return _leafElement.GetRefs(); }
        public override Result VirtVisit<Result>(Visitor<Result> actual) { return actual.ContextRef(this); }
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