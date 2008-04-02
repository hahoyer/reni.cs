using Reni.Context;

namespace Reni.Code
{
    /// <summary>
    /// Combination of TopRef and Unref
    /// </summary>
    sealed public class TopData : Top
    {
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

        /// <summary>
        /// Initializes a new instance of the <see cref="Top"/> class.
        /// </summary>
        /// <param name="refAlignParam">The ref align param.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="targetSize">Size of the target.</param>
        /// <param name="destinationSize">Size of the destination.</param>
        /// created 19.10.2006 22:02
        public TopData(RefAlignParam refAlignParam, Size offset, Size targetSize, Size destinationSize) : base(refAlignParam, offset, targetSize, destinationSize)
        {
            StopByObjectId(-1130);
        }
    }

    /// <summary>
    /// Combination of FrameRef and Unref
    /// </summary>
    sealed public class TopFrame : Top
    {
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

        /// <summary>
        /// Initializes a new instance of the <see cref="TopFrame"/> class.
        /// </summary>
        /// <param name="refAlignParam">The ref align param.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="targetSize">Size of the target.</param>
        /// <param name="destinationSize">Size of the destination.</param>
        /// created 19.10.2006 22:02
        /// created 04.01.2007 16:36
        public TopFrame(RefAlignParam refAlignParam, Size offset, Size targetSize, Size destinationSize)
            : base(refAlignParam, offset, targetSize, destinationSize)
        {
            StopByObjectId(544);
        }
    }
}