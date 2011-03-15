using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;

namespace Reni.Code
{
    /// <summary>
    ///     Code for end of statement
    /// </summary>
    [Serializable]
    internal sealed class LocalBlockEnd : FiberItem
    {
        [Node]
        private readonly Size _intermediateSize;

        private readonly Size _size;

        public LocalBlockEnd(Size size, Size intermediateSize)
        {
            Tracer.Assert(!intermediateSize.IsZero);

            _intermediateSize = intermediateSize;
            _size = size;
            StopByObjectId(166);
        }

        internal override Size InputSize { get { return _intermediateSize + _size; } }
        internal override Size OutputSize { get { return _intermediateSize + _size; } }

        protected override void Execute(IFormalMaschine formalMaschine) { formalMaschine.LocalBlockEnd(OutputSize, _intermediateSize); }

        [IsDumpEnabled(false)]
        public override string NodeDump { get { return base.NodeDump + " IntermediateSize=" + _intermediateSize; } }
    }
}