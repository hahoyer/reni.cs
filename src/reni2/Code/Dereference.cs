using HWClassLibrary.Debug;
using Reni.Context;

namespace Reni.Code
{
    /// <summary>
    /// Dereferencing operation
    /// </summary>
    internal sealed class Dereference : LeafElement
    {
        private readonly RefAlignParam _refAlignParam;
        private readonly Size _size;

        /// <summary>
        /// Initializes a new instance of the <see cref="Dereference"/> class.
        /// </summary>
        /// <param name="refAlignParam">The ref align param.</param>
        /// <param name="targetSize">Size of the target.</param>
        /// <param name="destinationSize">Size of the destination.</param>
        public Dereference(RefAlignParam refAlignParam, Size size)
        {
            _refAlignParam = refAlignParam;
            _size = size;
            StopByObjectId(399);
        }

        /// <summary>
        /// Gets the ref align param.
        /// </summary>
        /// <value>The ref align param.</value>
        /// created 05.10.2006 23:25
        public override RefAlignParam RefAlignParam { get { return _refAlignParam; } }

        /// <summary>
        /// Gets the size.
        /// </summary>
        /// <value>The size.</value>
        /// created 05.10.2006 23:40
        public override Size Size { get { return _size; } }

        /// <summary>
        /// Gets the size of the delta.
        /// </summary>
        /// <value>The size of the delta.</value>
        /// created 10.10.2006 00:21
        public override Size DeltaSize { get { return RefAlignParam.RefSize - Size; } }

        /// <summary>
        /// Formats this instance.
        /// </summary>
        /// <returns></returns>
        /// created 07.10.2006 21:11
        protected override string Format(StorageDescriptor start)
        {
            return start.Unref(RefAlignParam, _size);
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

        /// <summary>
        /// Tries to combine Y.
        /// </summary>
        /// <param name="precedingElement">The not other.</param>
        /// <returns></returns>
        /// created 19.10.2006 21:38
        internal override LeafElement TryToCombineBack(TopRef precedingElement)
        {
            Tracer.Assert(RefAlignParam.Equals(precedingElement.RefAlignParam));
            return new TopData(RefAlignParam, precedingElement.Offset, Size);
        }

        /// <summary>
        /// Tries to combine Y.
        /// </summary>
        /// <param name="precedingElement">The not other.</param>
        /// <returns></returns>
        /// created 19.10.2006 21:38
        internal override LeafElement TryToCombineBack(FrameRef precedingElement)
        {
            Tracer.Assert(RefAlignParam.Equals(precedingElement.RefAlignParam));
            return new TopFrame(RefAlignParam, precedingElement.Offset, Size);
        }
    }
}