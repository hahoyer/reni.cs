using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Code;

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

        [DumpData(false)]
        public override IContextRefInCode ForCode { get { return _context; } }
        [Node]
        internal override int Position { get { return _position; } }
        [DumpData(false),Node]
        internal override FullContext Context { get { return _context; } }
        internal override string DumpShort() { return base.DumpShort() + "@" + Position; }
        internal override IStructContext FindStruct() { return this; }
    }
}