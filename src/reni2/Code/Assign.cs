﻿using System;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using Reni.Context;

namespace Reni.Code
{
    [Serializable]
    internal class Assign : LeafElement
    {
        [IsDumpEnabled(false)]
        private readonly RefAlignParam _refAlignParam;

        public override string NodeDump { get { return base.NodeDump + " TargetSize=" + _targetSize + " RefSize=" + _refAlignParam.RefSize; } }
        internal override void Execute(IFormalMaschine formalMaschine) { formalMaschine.Assign(_targetSize, _refAlignParam); }

        [IsDumpEnabled(false), Node]
        private readonly Size _targetSize;

        public Assign(RefAlignParam refAlignParam, Size targetSize)
        {
            _refAlignParam = refAlignParam;
            _targetSize = targetSize;
        }

        protected override Size GetSize() { return Size.Zero; }

        protected override Size GetInputSize() { return _refAlignParam.RefSize*2; }

        protected override string Format(StorageDescriptor start) { return start.CreateAssignment(_refAlignParam, _targetSize); }
    }
}