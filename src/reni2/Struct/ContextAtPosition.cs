using HWClassLibrary.TreeStructure;
using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Code;
using Reni.Type;

namespace Reni.Struct
{
    [Serializable]
    internal sealed class ContextAtPosition : StructContextBase
    {
        private readonly int _position;
        private readonly FullContext _context;

        internal ContextAtPosition(FullContext context, int position)
            : base(context.Parent, context.Container)
        {
            _position = position;
            _context = context;
        }

        public override Ref NaturalRefType
        {
            get { return _context.NaturalRefType; }
        }

        [DumpData(false)]
        public override IRefInCode ForCode { get { return _context; } }

        [Node]
        protected override int Position { get { return _position; } }

        [DumpData(false), Node]
        internal override FullContext Context { get { return _context; } }

        internal override ContextAtPosition CreatePosition(int position) { return _context.CreatePosition(position); }
        internal override string DumpShort() { return base.DumpShort() + "@" + Position; }
        internal override IStructContext FindStruct() { return this; }

        [DumpData(false)]
        public override string NodeDump { get { return base.NodeDump + ": " + _context.NodeDump + "@" + _position; } }
    }
}