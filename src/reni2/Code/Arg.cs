using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Type;

namespace Reni.Code
{
    /// <summary>
    ///     Arg is is used as a placeholder.
    /// </summary>
    internal sealed class Arg : FiberHead
    {
        private static int _nextObjectId;
        private readonly TypeBase _type;

        internal Arg(TypeBase type)
            : base(_nextObjectId++)
        {
            _type = type;
            StopByObjectId(-1);
        }

        internal TypeBase Type { get { return _type; } }

        protected override Size GetSize() { return _type.Size; }
        protected override TResult VisitImplementation<TResult>(Visitor<TResult> actual) { return actual.Arg(this); }
    }
}