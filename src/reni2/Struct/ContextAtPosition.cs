using HWClassLibrary.Helper;
using HWClassLibrary.TreeStructure;
using System;
using HWClassLibrary.Debug;
using Reni.Code;

namespace Reni.Struct
{
    [Serializable]
    internal sealed class ContextAtPosition : Context
    {
        private readonly int _position;
        private readonly FullContext _context;
        private readonly SimpleCache<ThisType> _thisTypeCache; 

        internal ContextAtPosition(FullContext context, int position)
            : base(context.Parent, context.Container)
        {
            _position = position;
            _context = context;
            _thisTypeCache = new SimpleCache<ThisType>(() => new ThisType(this));
        }

        [DumpData(false)]
        public override IRefInCode ForCode { get { return _context; } }

        [Node]
        protected override int Position { get { return _position; } }

        [DumpData(false)]
        internal override ThisType ThisType { get { return _thisTypeCache.Value; } }

        internal override ContextAtPosition CreatePosition(int position) { return _context.CreatePosition(position); }
        internal override string DumpShort() { return base.DumpShort() + "@" + Position; }
        internal override IStructContext FindStruct() { return this; }

        [DumpData(false)]
        public override string NodeDump { get { return base.NodeDump + ": " + _context.NodeDump + "@" + _position; } }
    }
}