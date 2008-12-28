using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;

namespace Reni.Code
{
    /// <summary>
    /// Pair of code elements, first element can be accessed
    /// </summary>
    internal sealed class Pair : CodeBase
    {
        private readonly CodeBase _left;
        private readonly CodeBase _right;

        internal Pair(CodeBase left, CodeBase right)
        {
            _left = left;
            _right = right;
            var bc = Left.Size.SaveByteCount;
            StopByObjectId(1720);
        }

        [Node]
        internal CodeBase Left { get { return _left; } }
        [Node]
        internal CodeBase Right { get { return _right; } }

        internal protected override Size GetSize()
        {
            return Left.Size + Right.Size;
        }

        internal override Refs GetRefs()
        {
            return _left.GetRefs().CreateSequence(_right.GetRefs());
        }

        public override T VirtVisit<T>(Visitor<T> actual)
        {
            return actual.PairVisit(this);
        }

        internal protected override Size GetMaxSize()
        {
            var lSize = Left.MaxSize;
            var rSize = Left.Size + Right.MaxSize;
            return lSize.Max(rSize);
        }

        public static CodeBase ReCreate(CodeBase left, CodeBase right)
        {
            return new Pair(left, right);
        }
    }
}