using Reni.Context;

namespace Reni.Code
{
    /// <summary>
    /// Combination of TopRef and Unref
    /// </summary>
    internal sealed class TopData : Top
    {
        public TopData(RefAlignParam refAlignParam, Size offset, Size targetSize, Size destinationSize)
            : base(refAlignParam, offset, targetSize, destinationSize)
        {
            StopByObjectId(411);
        }
        /// <summary>
        /// Tries to combine two leaf elements. .
        /// </summary>
        /// <param name="subsequentElement">the element that follows.</param>
        /// <returns>null if no combination possible (default) or a leaf element that contains the combination of both</returns>
        /// created 19.10.2006 21:18
        internal override LeafElement TryToCombine(LeafElement subsequentElement)
        {
            return subsequentElement.TryToCombineBack(this);
        }

        /// <summary>
        /// Formats this instance.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <returns></returns>
        /// created 07.10.2006 21:11
        protected override string Format(StorageDescriptor start)
        {
            return start.TopData(RefAlignParam,Offset,TargetSize,DestinationSize);
        }
    }

    /// <summary>
    /// Combination of FrameRef and Unref
    /// </summary>
    internal sealed class TopFrame : Top
    {
        public TopFrame(RefAlignParam refAlignParam, Size offset, Size targetSize, Size destinationSize)
            : base(refAlignParam, offset, targetSize, destinationSize)
        {
            StopByObjectId(544);
        }
        /// <summary>
        /// Tries to combine two leaf elements. .
        /// </summary>
        /// <param name="subsequentElement">the element that follows.</param>
        /// <returns>null if no combination possible (default) or a leaf element that contains the combination of both</returns>
        /// created 19.10.2006 21:18
        internal override LeafElement TryToCombine(LeafElement subsequentElement)
        {
            return subsequentElement.TryToCombineBack(this);
        }

        /// <summary>
        /// Formats this instance.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <returns></returns>
        /// created 07.10.2006 21:11
        protected override string Format(StorageDescriptor start)
        {
            return start.TopFrame(RefAlignParam, Offset, TargetSize, DestinationSize);
        }

    }
}