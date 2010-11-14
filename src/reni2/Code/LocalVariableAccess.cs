using System;
using HWClassLibrary.Debug;
using Reni.Context;

namespace Reni.Code
{
    internal sealed class LocalVariableAccess : FiberHead
    {
        private readonly RefAlignParam _refAlignParam;
        private readonly string _holder;
        private readonly Size _offset;
        private readonly Size _size;

        public LocalVariableAccess(RefAlignParam refAlignParam, string holder, Size offset, Size size)
        {
            _refAlignParam = refAlignParam;
            _holder = holder;
            _offset = offset;
            _size = size;
        }

        internal override RefAlignParam RefAlignParam { get { return _refAlignParam; } }

        [IsDumpEnabled(false)]
        public override string NodeDump
        {
            get
            {
                return base.NodeDump
                       + " Holder=" + _holder 
                       + " Offset=" + _offset
                       + " Size=" + _size
                       ;
            }
        }
        protected override Size GetSize() { return _size; }
        protected override string CSharpString() { return CSharpGenerator.LocalVariableAccess(_holder,_offset, _size); }
    }
}