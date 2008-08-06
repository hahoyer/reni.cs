using System;
using Reni.Context;

namespace Reni.Code
{
    /// <summary>
    /// Reference to top of stack
    /// </summary>
    [Serializable]
    internal sealed class TopRef : RefCode
    {
        /// <summary>
        /// Initializes a new instance of the TopRef class.
        /// </summary>
        /// <param name="refAlignParam">The ref align param.</param>
        /// <param name="offset">The offset.</param>
        /// created 05.10.2006 23:26
        public TopRef(RefAlignParam refAlignParam, Size offset) : base(refAlignParam,offset)
        {
            StopByObjectId(715);
        }

        /// <summary>
        /// Formats this instance.
        /// </summary>
        /// <returns></returns>
        /// created 07.10.2006 21:11
        protected override string Format(StorageDescriptor start)
        {
            return start.TopRef(RefAlignParam, Offset);
        }

        /// <summary>
        /// Tries to combine.
        /// </summary>
        /// <param name="subsequentElement">The other.</param>
        /// <returns></returns>
        /// created 19.10.2006 21:18
        internal override LeafElement TryToCombine(LeafElement subsequentElement)
        {
            return subsequentElement.TryToCombineBack(this);
        }
    }
    /// <summary>
    /// Reference to the end of argument array
    /// </summary>
    [Serializable]
    internal sealed class FrameRef : RefCode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TopRef"/> class.
        /// </summary>
        /// <param name="refAlignParam">The ref align param.</param>
        /// <param name="offset">The offset.</param>
        /// created 05.10.2006 23:26
        public FrameRef(RefAlignParam refAlignParam, Size offset)
            : base(refAlignParam, offset)
        {
            StopByObjectId(547);
        }

        /// <summary>
        /// Formats this instance.
        /// </summary>
        /// <returns></returns>
        /// created 07.10.2006 21:11
        protected override string Format(StorageDescriptor start)
        {
            return start.FrameRef(RefAlignParam, Offset);
        }

        /// <summary>
        /// Tries to combine.
        /// </summary>
        /// <param name="subsequentElement">The other.</param>
        /// <returns></returns>
        /// created 19.10.2006 21:18
        internal override LeafElement TryToCombine(LeafElement subsequentElement)
        {
            return subsequentElement.TryToCombineBack(this);
        }
    }
}