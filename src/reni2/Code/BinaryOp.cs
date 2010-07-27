using System;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;

namespace Reni.Code
{
    /// <summary>
    /// Binary operations
    /// </summary>
    [Serializable]
    internal abstract class BinaryOp : LeafElement
    {
        [DumpData(false)]
        internal protected readonly Size LeftSize;
        [DumpData(false)]
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