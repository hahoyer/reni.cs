using System;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using Reni.Context;

namespace Reni.Code
{
    [Serializable]
    internal sealed class Assign : FiberItem
    {
        [IsDumpEnabled(false)]
        private readonly RefAlignParam _refAlignParam;

        public override string NodeDump { get { return base.NodeDump + " TargetSize=" + _targetSize + " RefSize=" + _refAlignParam.RefSize; } }
        protected override void Execute(IFormalMaschine formalMaschine) { formalMaschine.Assign(_targetSize, _refAlignParam); }

        [IsDumpEnabled(false), Node]
        private readonly Size _targetSize;

        public Assign(RefAlignParam refAlignParam, Size targetSize)
        {
            _refAlignParam = refAlignParam;
            _targetSize = targetSize;
        }

        [IsDumpEnabled(false)]
        internal override Size InputSize { get { return _refAlignParam.RefSize * 2; } }
        [IsDumpEnabled(false)]
        internal override Size OutputSize { get { return Size.Zero; } }
    }
}