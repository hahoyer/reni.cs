using HWClassLibrary.Debug;

namespace Reni.Code
{
    /// <summary>
    /// Code for end of statement
    /// </summary>
    internal sealed class StatementEnd : LeafElement
    {
        private readonly Size _intermediateSize;
        private readonly Size _size;

        public StatementEnd(Size size, Size intermediateSize)
        {
            Tracer.Assert(!intermediateSize.IsZero);

            _intermediateSize = intermediateSize;
            _size = size;
            StopByObjectId(166);
        }

        public Size IntermediateSize { get { return _intermediateSize; } }
        public override Size Size { get { return _size; } }
        public override Size DeltaSize { get { return IntermediateSize; } }

        protected override string Format(StorageDescriptor start)
        {
            return start.StatementEnd(Size, IntermediateSize);
        }

    }
}