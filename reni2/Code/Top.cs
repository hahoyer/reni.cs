using Reni.Context;

namespace Reni.Code
{
    /// <summary>
    /// 
    /// </summary>
    abstract public class Top : LeafElement
    {
        private readonly RefAlignParam _refAlignParam;
        private readonly Size _offset;
        private readonly Size _targetSize;
        private readonly Size _destinationSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Top"/> class.
        /// </summary>
        /// <param name="refAlignParam">The ref align param.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="targetSize">Size of the target.</param>
        /// <param name="destinationSize">Size of the destination.</param>
        /// created 19.10.2006 22:02
        public Top(RefAlignParam refAlignParam, Size offset, Size targetSize, Size destinationSize)
        {
            _refAlignParam = refAlignParam;
            _offset = offset;
            _targetSize = targetSize;
            _destinationSize = destinationSize;
            StopByObjectId(945);
        }

        /// <summary>
        /// Gets the size.
        /// </summary>
        /// <value>The size.</value>
        /// created 05.10.2006 23:40
        public override Size Size { get { return _destinationSize; } }

        /// <summary>
        /// Gets the size of the delta.
        /// </summary>
        /// <value>The size of the delta.</value>
        /// created 10.10.2006 00:21
        public override Size DeltaSize { get { return Size*(-1); } }

        /// <summary>
        /// Gets the ref align param.
        /// </summary>
        /// <value>The ref align param.</value>
        /// created 19.10.2006 20:03
        /// created 19.10.2006 22:02
        public override RefAlignParam RefAlignParam { get { return _refAlignParam; } }

        /// <summary>
        /// Gets the offset.
        /// </summary>
        /// <value>The offset.</value>
        /// created 04.01.2007 15:16
        public Size Offset { get { return _offset; } }

        /// <summary>
        /// Gets the size of the target.
        /// </summary>
        /// <value>The size of the target.</value>
        /// created 04.01.2007 15:16
        public Size TargetSize { get { return _targetSize; } }

        /// <summary>
        /// Gets the size of the destination.
        /// </summary>
        /// <value>The size of the destination.</value>
        /// created 04.01.2007 15:16
        public Size DestinationSize { get { return _destinationSize; } }
    }
}