using System;

namespace Reni.Code
{
    /// <summary>
    /// Arg is is used as a placeholder.
    /// </summary>
    internal sealed class Arg : CodeBase
    {
        private static int _nextObjectId;
        private readonly Size _size;

        internal Arg(Size size)
            :base(_nextObjectId++)
        {
            _size = size;
            StopByObjectId(-2);
        }

        protected override Size SizeImplementation { get { return _size; } }
        protected override TResult VisitImplementation<TResult>(Visitor<TResult> actual) { return actual.Arg(this); }
    }
}