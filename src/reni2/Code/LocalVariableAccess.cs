﻿using System;
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
        private readonly Size _dataSize;

        public LocalVariableAccess(RefAlignParam refAlignParam, string holder, Size offset, Size size, Size dataSize)
        {
            _refAlignParam = refAlignParam;
            _holder = holder;
            _offset = offset;
            _size = size;
            _dataSize = dataSize;
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
                       + " DataSize=" + _dataSize
                       ;
            }
        }
        protected override Size GetSize() { return _size; }
        protected override string CSharpString() { return CSharpGenerator.LocalVariableAccess(_holder,_offset, _size); }
        protected override void Execute(IFormalMaschine formalMaschine) { formalMaschine.LocalVariableAccess(Size, _holder, _offset, _dataSize); }
    }
}