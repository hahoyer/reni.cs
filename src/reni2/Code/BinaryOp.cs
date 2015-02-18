using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Forms;
using Reni.Basics;

namespace Reni.Code
{
    /// <summary>
    ///     Binary operations
    /// </summary>
    abstract class BinaryOp : FiberItem
    {
        [DisableDump]
        [Node]
        internal readonly Size LeftSize;

        [DisableDump]
        [Node]
        internal readonly Size RightSize;

        protected BinaryOp(Size leftSize, Size rightSize)
        {
            LeftSize = leftSize;
            RightSize = rightSize;
        }

        [DisableDump]
        internal override Size InputSize => LeftSize + RightSize;
    }
}