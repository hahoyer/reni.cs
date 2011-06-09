using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;

namespace Reni.Code
{
    internal sealed class LocalVariableDefinition : FiberItem
    {
        private readonly string _holderName;
        private readonly Size _valueSize;

        public LocalVariableDefinition(string holderName, Size valueSize)
        {
            _holderName = holderName;
            _valueSize = valueSize;
        }

        [Node, DisableDump]
        internal override Size InputSize { get { return _valueSize; } }

        [Node, DisableDump]
        internal override Size OutputSize { get { return Size.Zero; } }

        [Node, DisableDump]
        internal string HolderName { get { return _holderName; } }

        [Node, DisableDump]
        public Size ValueSize { get { return _valueSize; } }

        public override string NodeDump { get { return base.NodeDump + " Holder=" + _holderName + " ValueSize=" + _valueSize; } }
        protected override void Execute(IFormalMaschine formalMaschine) { formalMaschine.LocalVariableDefinition(HolderName, _valueSize); }
    }
}