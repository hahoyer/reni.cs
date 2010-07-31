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
        private static int _nextObjectId = 1;
        internal RefCode(IRefInCode context):base(_nextObjectId++)
        {
            _leafElement = new ContextRef(context);
            StopByObjectId(-38);
        }

        [Node]
        internal IRefInCode Context { get { return _leafElement.Context; } }

        [DumpData(false)]
        internal LeafElement ToLeafElement { get { return _leafElement; } }

        [DumpData(false)]
        protected override Size SizeImplementation { get { return _leafElement.Size; } }
        [DumpData(false)]
        internal override Refs RefsImplementation { get { return _leafElement.GetRefs(); } }

        protected override TResult VisitImplementation<TResult>(Visitor<TResult> actual) { return actual.ContextRef(this); }
    }

    [Serializable]
    internal class ContextRef : LeafElement
    {
        [Node]
        internal readonly IRefInCode Context;

        public ContextRef(IRefInCode context) { Context = context; }
        protected override Size GetSize() { return Context.RefAlignParam.RefSize; }
        protected override Size GetInputSize() { return Size.Zero; }
        internal Refs GetRefs() { return Refs.Create(Context); }
    }
}