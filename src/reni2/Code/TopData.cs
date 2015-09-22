using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Basics;

namespace Reni.Code
{
    /// <summary>
    ///     Combination of TopRef and Unref
    /// </summary>
    sealed class TopData : Top
    {
        public TopData(Size offset, Size size, Size dataSize)
            : base(offset, size, dataSize)
        {
            StopByObjectIds(-110);
        }

        protected override CodeBase TryToCombine(FiberItem subsequentElement)
            => subsequentElement.TryToCombineBack(this);

        internal override void Visit(IVisitor visitor) => visitor.TopData(Offset, Size, DataSize);
    }

    /// <summary>
    ///     Combination of TopFrameRef and Unref
    /// </summary>
    sealed class TopFrameData : Top
    {
        public TopFrameData(Size offset, Size size, Size dataSize)
            : base(offset, size, dataSize)
        {
        }

        protected override CodeBase TryToCombine(FiberItem subsequentElement)
            => subsequentElement.TryToCombineBack(this);

        internal override void Visit(IVisitor visitor)
            => visitor.TopFrameData(Offset, Size, DataSize);
    }
}