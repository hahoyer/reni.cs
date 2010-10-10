using System;
using HWClassLibrary.Debug;

namespace Reni.Code
{
    internal class Drop : FiberItem
    {
        private readonly Size _beforeSize;
        private readonly Size _afterSize;

        public Drop(Size beforeSize, Size afterSize)
        {
            _beforeSize = beforeSize;
            _afterSize = afterSize;
        }

        internal override void Execute(IFormalMaschine formalMaschine) { formalMaschine.Drop(_beforeSize, _afterSize); }
        [IsDumpEnabled(false)]
        internal override Size InputSize { get { return _beforeSize; } }
        [IsDumpEnabled(false)]
        internal override Size OutputSize { get { return _afterSize; } }
        protected override string Format(StorageDescriptor start) { return ""; }
        [IsDumpEnabled(false)]
        public override string NodeDump { get { return base.NodeDump + " BeforeSize=" + _beforeSize + " AfterSize=" + _afterSize; } }
    }
}