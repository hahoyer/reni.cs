using HWClassLibrary.TreeStructure;
using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;

namespace Reni.Code
{
    /// <summary>
    /// Code for end of statement
    /// </summary>
    [Serializable]
    internal sealed class LocalBlockEnd : LeafElement
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

        protected override Size GetSize()
        {
            return _intermediateSize + _size;
        }

        protected override Size GetInputSize()
        {
            return _intermediateSize + _size;
        }

        protected override string Format(StorageDescriptor start)
        {
            return start.CreateLocalBlockEnd(_size, _intermediateSize);
        }

        internal override void Execute(IFormalMaschine formalMaschine) { formalMaschine.LocalBlockEnd(Size, _intermediateSize); }

        public override string NodeDump { get { return base.NodeDump + " IntermediateSize=" + _intermediateSize; } }
    }
}