using System;

namespace Reni.Code
{
    /// <summary>
    /// Binary operations
    /// </summary>
    [Serializable]
    internal abstract class BinaryOp : LeafElement
    {
        internal protected readonly Size LeftSize;
        internal protected readonly Size RightSize;

        protected BinaryOp(Size leftSize, Size rightSize)
        {
            LeftSize = leftSize;
            RightSize = rightSize;
        }

        protected override Size GetInputSize()
        {
            return LeftSize + RightSize;
        }
    }
}