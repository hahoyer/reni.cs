using hw.DebugFormatter;
using Reni.Basics;
using Reni.Context;

namespace Reni.Code
{
    sealed class DePointer : FiberItem
    {
        static int NextObjectId;

        public DePointer(Size outputSize, Size dataSize)
            : base(NextObjectId++)
        {
            OutputSize = outputSize;
            DataSize = dataSize;
            StopByObjectIds();
        }

        [DisableDump]
        internal Size DataSize { get; }

        protected override string GetNodeDump() => base.GetNodeDump() + " DataSize=" + DataSize;

        [DisableDump]
        internal override Size InputSize => Root.DefaultRefAlignParam.RefSize;

        [DisableDump]
        internal override Size OutputSize { get; }

        protected override FiberItem[] TryToCombineImplementation(FiberItem subsequentElement)
            => subsequentElement.TryToCombineBack(this);

        internal override CodeBase TryToCombineBack(TopRef precedingElement)
            => new TopData(precedingElement.Offset, OutputSize, DataSize);

        internal override CodeBase TryToCombineBack(LocalReference precedingElement)
        {
            if(precedingElement.ValueType.Size == DataSize)
                return precedingElement.ValueCode.BitCast(OutputSize);
            return null;
        }

        internal override CodeBase TryToCombineBack(TopFrameRef precedingElement)
            => new TopFrameData(precedingElement.Offset, OutputSize, DataSize);

        internal override void Visit(IVisitor visitor) => visitor.DePointer(OutputSize, DataSize);

        protected override TFiber VisitImplementation<TCode, TFiber>(Visitor<TCode, TFiber> actual)
            => actual.DePointer(this);
    }
}