using HWClassLibrary.Debug;
using Reni.Context;

namespace Reni.Code
{
    /// <summary>
    /// Reference shift
    /// </summary>
    sealed public class RefPlus : LeafElement
    {
        private readonly RefAlignParam _refAlignParam;
        private readonly Size _right;

        /// <summary>
        /// Gets the ref align param.
        /// </summary>
        /// <value>The ref align param.</value>
        /// created 19.10.2006 19:20
        public override RefAlignParam RefAlignParam { get { return _refAlignParam; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="RefPlus"/> class.
        /// </summary>
        /// <param name="refAlignParam">The ref align param.</param>
        /// <param name="right">The right.</param>
        /// created 28.09.2006 22:00
        public RefPlus(RefAlignParam refAlignParam, Size right)
        {
            _refAlignParam = refAlignParam;
            _right = right;
            StopByObjectId(467);
        }

        /// <summary>
        /// Gets the right.
        /// </summary>
        /// <value>The right.</value>
        /// created 28.09.2006 22:00
        public Size Right { get { return _right; } }


        /// <summary>
        /// Gets the size.
        /// </summary>
        /// <value>The size.</value>
        /// created 23.09.2006 14:15
        public override Size Size { get { return RefAlignParam.RefSize; } }

        /// <summary>
        /// Gets the size of the delta.
        /// </summary>
        /// <value>The size of the delta.</value>
        /// created 10.10.2006 00:21
        public override Size DeltaSize { get { return Size.Create(0); } }

        /// <summary>
        /// Formats this instance.
        /// </summary>
        /// <returns></returns>
        /// created 07.10.2006 21:11
        protected override string Format(StorageDescriptor start)
        {
            return start.RefPlus(Size, Right.SizeToPacketCount(RefAlignParam.AlignBits));
        }

        /// <summary>
        /// Tries to combine Y.
        /// </summary>
        /// <param name="precedingElement">The not other.</param>
        /// <returns></returns>
        /// created 19.10.2006 21:38
        internal override LeafElement TryToCombineBack(TopRef precedingElement)
        {
            //return null;
            Tracer.Assert(RefAlignParam.Equals(precedingElement.RefAlignParam));
            return new TopRef(RefAlignParam, precedingElement.Offset + Right);
        }

        /// <summary>
        /// Tries to combine Y.
        /// </summary>
        /// <param name="precedingElement">The not other.</param>
        /// <returns></returns>
        /// created 19.10.2006 21:38
        internal override LeafElement TryToCombineBack(FrameRef precedingElement)
        {
            return null;
            Tracer.Assert(RefAlignParam.Equals(precedingElement.RefAlignParam));
            return new FrameRef(RefAlignParam, precedingElement.Offset + Right);
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
        /// Tries to combine back.
        /// </summary>
        /// <param name="precedingElement">The preceding element.</param>
        /// <returns></returns>
        /// created 04.01.2007 17:55
        internal override LeafElement TryToCombineBack(RefPlus precedingElement)
        {
            if (RefAlignParam.IsEqual(precedingElement.RefAlignParam))
                return new RefPlus(RefAlignParam, Right + precedingElement.Right);
            return base.TryToCombineBack(precedingElement);
        }
    }
}