using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Basics;

namespace Reni.Code
{
    /// <summary>
    ///     Arg is is used as a placeholder.
    /// </summary>
    internal sealed class Arg : FiberHead
    {
        private static int _nextObjectId;
        private readonly Size _size;

        internal Arg(Size size)
            : base(_nextObjectId++)
        {
            _size = size;
            StopByObjectId(-9);
        }

        protected override Size GetSize() { return _size; }
        protected override TResult VisitImplementation<TResult>(Visitor<TResult> actual) { return actual.Arg(this); }
    }
}