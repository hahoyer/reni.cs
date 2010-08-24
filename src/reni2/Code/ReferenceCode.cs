using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;

namespace Reni.Code
{
    /// <summary>
    /// ContextAtPosition reference, should be replaced
    /// </summary>
    internal sealed class ReferenceCode : CodeBase
    {
        private readonly ContextRef _leafElement;
        private static int _nextObjectId;
        internal ReferenceCode(IReferenceInCode context):base(_nextObjectId++)
        {
            _leafElement = new ContextRef(context);
            StopByObjectId(-15);
        }

        [Node]
        internal IReferenceInCode Context { get { return _leafElement.Context; } }
        [IsDumpEnabled(false)]
        internal LeafElement ToLeafElement { get { return _leafElement; } }
        [IsDumpEnabled(false)]
        protected override Size SizeImplementation { get { return _leafElement.Size; } }
        [IsDumpEnabled(false)]
        internal override Refs RefsImplementation { get { return _leafElement.GetRefs(); } }

        protected override TResult VisitImplementation<TResult>(Visitor<TResult> actual) { return actual.ContextRef(this); }
    }
}