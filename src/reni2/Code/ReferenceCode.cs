using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;

namespace Reni.Code
{
    /// <summary>
    /// ContextAtPosition reference, should be replaced
    /// </summary>
    internal sealed class ReferenceCode : FiberHead
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
        internal ContextRef ToLeafElement { get { return _leafElement; } }

        protected override Size GetSize() { return _leafElement.OutputSize; }

        [IsDumpEnabled(false)]
        internal override Refs RefsImplementation { get { return _leafElement.GetRefs(); } }

        protected override TResult VisitImplementation<TResult>(Visitor<TResult> actual) { return actual.ContextRef(this); }
    }
}