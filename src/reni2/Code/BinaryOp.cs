using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;

namespace Reni.Code
{
    /// <summary>
    ///     Binary operations
    /// </summary>
    [Serializable]
    internal abstract class BinaryOp : FiberItem
    {
        [DisableDump]
        protected internal readonly Size LeftSize;

        [DisableDump]
        protected internal readonly Size RightSize;

        protected BinaryOp(Size leftSize, Size rightSize)
        {
            LeftSize = leftSize;
            RightSize = rightSize;
        }

        [DisableDump]
        internal override Size InputSize { get { return LeftSize + RightSize; } }
    }
}