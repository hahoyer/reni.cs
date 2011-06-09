using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;

namespace Reni.Code
{
    /// <summary>
    ///     Pair of code elements, first element can be accessed
    /// </summary>
    [Obsolete]
    internal sealed class Pair : FiberHead
    {
        private readonly CodeBase _left;
        private readonly CodeBase _right;
        private static int _nextObjectId;

        internal Pair(CodeBase left, CodeBase right)
            : base(_nextObjectId++)
        {
            _left = left;
            _right = right;
            var bc = Left.Size.SaveByteCount;
            StopByObjectId(-8);
        }

        [Node]
        internal CodeBase Left { get { return _left; } }

        [Node]
        internal CodeBase Right { get { return _right; } }

        protected override Size GetSize() { return Left.Size + Right.Size; }
        protected override Refs GetRefsImplementation() { return _left.Refs.Sequence(_right.Refs); }

        [DisableDump]
        protected override Size MaxSizeImplementation
        {
            get
            {
                var lSize = Left.MaxSize;
                var rSize = Left.Size + Right.MaxSize;
                return lSize.Max(rSize);
            }
        }

        public override string DumpData() { return InternalDumpData(""); }

        private static string InternalDumpData(string head, CodeBase x)
        {
            var pair = x as Pair;
            if(pair == null)
                return head + "\n" + x.Dump();
            return pair.InternalDumpData(head);
        }

        private string InternalDumpData(string head)
        {
            var newHead = ObjectId + "." + head;
            return InternalDumpData(newHead + "L", Left) + "\n" + InternalDumpData(newHead + "R", Right);
        }
    }
}