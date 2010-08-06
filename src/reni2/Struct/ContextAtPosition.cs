using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using Reni.Code;

namespace Reni.Struct
{
    [Serializable]
    internal sealed class ContextAtPosition : Context
    {
        private readonly int _position;
        private readonly FullContext _context;

        internal ContextAtPosition(FullContext context, int position)
            : base(context.Parent, context.Container)
        {
            _position = position;
            _context = context;
        }

        [DumpData(false)]
        protected override IRefInCode ForCode { get { return _context; } }

        [Node]
        protected override int Position { get { return _position; } }

        internal override ContextAtPosition CreatePosition(int position)
        {
            if (position < _position)
                return this;
            return _context.CreatePosition(position);
        }
        internal override string DumpShort() { return base.DumpShort() + "@" + Position; }

        [DumpData(false)]
        public override string NodeDump { get { return base.NodeDump + ": " + _context.NodeDump + "@" + _position; } }
    }
}