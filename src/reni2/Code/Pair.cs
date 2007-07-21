using System;
using HWClassLibrary.Debug;

namespace Reni.Code
{
    /// <summary>
    /// Pair of code elements, first element can be accessed
    /// </summary>
    public sealed class Pair : Base
	{
        private readonly Base _left;
        private readonly Base _right;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
		public Pair(Base left, Base right)
        {
            _left = left;
            _right = right;
            int bc = Left.Size.SaveByteCount;
        }

        /// <summary>
        /// Visitor to replace parts of code, overridable version. 
        /// </summary>
        /// <param name="actual"></param>
        /// <returns></returns>
        public override T VirtVisit<T>(Visitor<T> actual)
        {
            return actual.PairVisit(this);
        }

        /// <summary>
		/// Size of object
		/// </summary>
		public override Size Size { get { return Left.Size + Right.Size; } }

        public override Refs Refs { get { return _left.Refs.Pair(_right.Refs); } }

        /// <summary>
        /// Gets the max bytes.
        /// </summary>
        /// <value>The max bytes.</value>
        /// [created 20.07.2006 00:21]
        public override Size MaxSize
        {
            get
            {
                Size lSize = Left.MaxSize;
                Size rSize = Left.Size + Right.MaxSize;
                return lSize.Max(rSize);
            }
        }

        /// <summary>
        /// Gets the left.
        /// </summary>
        /// <value>The left.</value>
        /// created 06.10.2006 00:56
        public Base Left { get { return _left; } }
        /// <summary>
        /// Gets the right.
        /// </summary>
        /// <value>The right.</value>
        /// created 06.10.2006 00:56
        public Base Right { get { return _right; } }

        /// <summary>
        /// Res the create.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        /// created 02.10.2006 20:56
        public Base ReCreate(Base left, Base right)
        {
            return new Pair(left, right);
        }
	}
}