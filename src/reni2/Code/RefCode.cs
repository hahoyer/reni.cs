using HWClassLibrary.Debug;
using Reni.Context;

namespace Reni.Code
{
    /// <summary>
    /// Reference to something 
    /// </summary>
    internal abstract class RefCode : LeafElement
    {
        private readonly RefAlignParam _refAlignParam;
        private readonly Size _offset;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:RefCode"/> class.
        /// </summary>
        /// <param name="refAlignParam">The ref align param.</param>
        /// <param name="offset">The offset.</param>
        /// created 02.10.2006 20:55
        public RefCode(RefAlignParam refAlignParam, Size offset)
        {
            _refAlignParam = refAlignParam;
            _offset = offset;
        }

        /// <summary>
        /// Gets the size.
        /// </summary>
        /// <value>The size.</value>
        /// created 23.09.2006 14:15
        sealed public override Size Size { get { return _refAlignParam.RefSize; } }
        /// <summary>
        /// Gets the size of the delta.
        /// </summary>
        /// <value>The size of the delta.</value>
        /// created 10.10.2006 00:21
        public override Size DeltaSize { get { return Size * -1; } }

        /// <summary>
        /// Gets the ref align param.
        /// </summary>
        /// <value>The ref align param.</value>
        /// created 19.10.2006 20:03
        /// created 03.01.2007 22:27
        public override RefAlignParam RefAlignParam { get { return _refAlignParam; } }
        /// <summary>
        /// Gets the offset.
        /// </summary>
        /// <value>The offset.</value>
        /// created 03.01.2007 22:27
        public Size Offset { get { return _offset; } }
    }
}