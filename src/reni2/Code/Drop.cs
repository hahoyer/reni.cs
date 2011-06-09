using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;

namespace Reni.Code
{
    internal sealed class Drop : FiberItem
    {
        private readonly Size _beforeSize;
        private readonly Size _afterSize;

        public Drop(Size beforeSize, Size afterSize)
        {
            _beforeSize = beforeSize;
            _afterSize = afterSize;
        }

        protected override string CSharpCodeSnippet(Size top) { return CSharpGenerator.Drop(InputSize, OutputSize); }

        protected override void Execute(IFormalMaschine formalMaschine) { formalMaschine.Drop(_beforeSize, _afterSize); }

        [DisableDump]
        internal override Size InputSize { get { return _beforeSize; } }

        [DisableDump]
        internal override Size OutputSize { get { return _afterSize; } }

        [DisableDump]
        public override string NodeDump { get { return base.NodeDump + " BeforeSize=" + _beforeSize + " AfterSize=" + _afterSize; } }
    }
}