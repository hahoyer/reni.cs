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
        private readonly Size IntermediateSize;
        private readonly Size _size;

        public LocalBlockEnd(Size size, Size intermediateSize)
        {
            Tracer.Assert(!intermediateSize.IsZero);

            IntermediateSize = intermediateSize;
            _size = size;
            StopByObjectId(166);
        }

        protected override Size GetSize()
        {
            return IntermediateSize + _size;
        }

        protected override Size GetInputSize()
        {
            return IntermediateSize + _size;
        }

        protected override string Format(StorageDescriptor start)
        {
            return start.CreateLocalBlockEnd(_size, IntermediateSize);
        }

    }
}