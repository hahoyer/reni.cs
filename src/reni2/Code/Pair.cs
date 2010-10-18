using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;

namespace Reni.Code
{
    /// <summary>
    /// Pair of code elements, first element can be accessed
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

        [IsDumpEnabled(false)]
        internal override Refs RefsImplementation { get { return _left.RefsImplementation.Sequence(_right.RefsImplementation); } }

        [IsDumpEnabled(false)]
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

    internal sealed class List : FiberHead
    {
        [IsDumpEnabled(true)]
        private readonly CodeBase[] _data;

        internal static List Create(params CodeBase[] data) { return new List(data); }

        internal List(IEnumerable<CodeBase> data)
        {
            _data = data.ToArray();
            foreach (var codeBase in _data)
                Tracer.Assert(!(codeBase is List));
            Tracer.Assert(_data.Length > 1);
        }

        protected override IEnumerable<CodeBase> AsList() { return _data; }

        protected override Size GetSize() { return _data.Aggregate(Size.Zero, (size, codeBase) => size + codeBase.Size); }
    }
}