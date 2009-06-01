using HWClassLibrary.TreeStructure;
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

        [DumpData(false)]
        protected override Size SizeImplementation { get { return Left.Size + Right.Size; } }

        [DumpData(false)]
        internal override Refs RefsImplementation { get { return _left.RefsImplementation.CreateSequence(_right.RefsImplementation); } }

        public override T VisitImplementation<T>(Visitor<T> actual)
        {
            return actual.PairVisit(this);
        }

        [DumpData(false)]
        protected override Size MaxSizeImplementation
        {
            get
            {
                var lSize = Left.MaxSize;
                var rSize = Left.Size + Right.MaxSize;
                return lSize.Max(rSize);
            }
        }
    }
}