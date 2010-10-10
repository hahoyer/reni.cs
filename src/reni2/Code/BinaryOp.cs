using System;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;

namespace Reni.Code
{
    /// <summary>
    /// Binary operations
    /// </summary>
    [Serializable]
    internal abstract class BinaryOp : FiberItem
    {
        [IsDumpEnabled(false)]
        internal protected readonly Size LeftSize;
        [IsDumpEnabled(false)]
        internal protected readonly Size RightSize;

        protected BinaryOp(Size leftSize, Size rightSize)
        {
            LeftSize = leftSize;
            RightSize = rightSize;
        }

        [IsDumpEnabled(false)]
        internal override Size InputSize { get { return LeftSize + RightSize; } }
    }
}