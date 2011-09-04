using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using Reni.Basics;

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

        internal override void Visit(IVisitor visitor) { visitor.LocalBlockEnd(OutputSize, _intermediateSize); }

        [DisableDump]
        public override string NodeDump { get { return base.NodeDump + " IntermediateSize=" + _intermediateSize; } }
    }
}