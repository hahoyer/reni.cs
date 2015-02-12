using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Basics;
using Reni.Context;

namespace Reni.Code
{
    sealed class DePointer : FiberItem
    {
        static int _nextObjectId;

        public DePointer(Size outputSize, Size dataSize)
            : base(_nextObjectId++)
        {
            OutputSize = outputSize;
            DataSize = dataSize;
            StopByObjectId(-6);
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

        internal override CodeBase TryToCombineBack(LocalVariableReference precedingElement)
            => new LocalVariableAccess(precedingElement.Holder, precedingElement.Offset, OutputSize, DataSize);

        internal override CodeBase TryToCombineBack(TopFrameRef precedingElement)
            => new TopFrameData(precedingElement.Offset, OutputSize, DataSize);

        internal override void Visit(IVisitor visitor) => visitor.DePointer(OutputSize, DataSize);
    }
}