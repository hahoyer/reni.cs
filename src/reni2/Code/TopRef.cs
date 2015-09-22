using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Basics;

namespace Reni.Code
{
    sealed class TopRef : Ref
    {
        public TopRef(Size offset)
            : base(offset) { }

        public TopRef()
            : this(Size.Zero) { }

        protected override CodeBase TryToCombine(FiberItem subsequentElement) => subsequentElement.TryToCombineBack(this);

        internal override void Visit(IVisitor visitor) => visitor.TopRef(Offset);
    }

    sealed class TopFrameRef : Ref
    {
        public TopFrameRef()
            : this(Size.Zero) { }

        public TopFrameRef(Size offset)
            : base(offset) { }

        protected override CodeBase TryToCombine(FiberItem subsequentElement) => subsequentElement.TryToCombineBack(this);
        internal override void Visit(IVisitor visitor) => visitor.TopFrameRef(Offset);
    }
}