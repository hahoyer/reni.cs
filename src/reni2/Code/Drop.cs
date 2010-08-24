using HWClassLibrary.Debug;

namespace Reni.Code
{
    internal class Drop : LeafElement
    {
        private readonly Size _beforeSize;
        private readonly Size _afterSize;

        public Drop(Size beforeSize, Size afterSize)
        {
            _beforeSize = beforeSize;
            _afterSize = afterSize;
        }

        [IsDumpEnabled(false)]
        public override string NodeDump { get { return base.NodeDump + " BeforeSize=" + _beforeSize + " AfterSize=" + _afterSize; } }
        internal override void Execute(IFormalMaschine formalMaschine) { formalMaschine.Drop(_beforeSize, _afterSize); }

        protected override Size GetSize() { return _afterSize; }

        protected override Size GetInputSize() { return _beforeSize; }

        protected override string Format(StorageDescriptor start) { return ""; }
    }
}