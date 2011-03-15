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
        [IsDumpEnabled(false)]
        protected internal readonly Size LeftSize;

        [IsDumpEnabled(false)]
        protected internal readonly Size RightSize;

        protected BinaryOp(Size leftSize, Size rightSize)
        {
            LeftSize = leftSize;
            RightSize = rightSize;
        }

        [IsDumpEnabled(false)]
        internal override Size InputSize { get { return LeftSize + RightSize; } }
    }
}