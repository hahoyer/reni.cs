using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Basics;

namespace Reni.Code
{
    sealed class TopRef : Ref
    {
        public TopRef(Size offset)
            : base(offset) { StopByObjectIds(); }

        public TopRef()
            : this(Size.Zero) { }

        protected override CodeBase TryToCombine(FiberItem subsequentElement)
            => subsequentElement.TryToCombineBack(this);

        protected override TCode VisitImplementation<TCode, TFiber>(Visitor<TCode, TFiber> actual)
            => actual.TopRef(this);

        internal override void Visit(IVisitor visitor) => visitor.TopRef(Offset);
    }

    sealed class TopFrameRef : Ref
    {
        public TopFrameRef()
            : this(Size.Zero) { }

        public TopFrameRef(Size offset)
            : base(offset) { }

        protected override CodeBase TryToCombine(FiberItem subsequentElement)
            => subsequentElement.TryToCombineBack(this);

        internal override void Visit(IVisitor visitor) => visitor.TopFrameRef(Offset);
    }
}