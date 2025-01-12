using Reni.Basics;
using Reni.Context;

namespace Reni.Code
{
    abstract class ArrayAccess : FiberItem
    {
        [EnableDump]
        internal readonly Size ElementSize;

        [EnableDump]
        internal readonly Size IndexSize;

        [PublicAPI]
        new readonly string CallingMethodName;

        protected ArrayAccess(Size elementSize, Size indexSize, string callingMethodName)
        {
            ElementSize = elementSize;
            IndexSize = indexSize;
            CallingMethodName = callingMethodName;
        }
    }

    sealed class ArrayGetter : ArrayAccess
    {
        public ArrayGetter(Size elementSize, Size indexSize, string callingMethodName)
            : base(elementSize, indexSize, callingMethodName)
            => StopByObjectIds();

        [DisableDump]
        internal override Size InputSize => Root.DefaultRefAlignParam.RefSize + IndexSize;

        [DisableDump]
        internal override Size OutputSize => Root.DefaultRefAlignParam.RefSize;

        internal override void Visit(IVisitor visitor)
            => visitor.ArrayGetter(ElementSize, IndexSize);

        protected override TFiber VisitImplementation<TCode, TFiber>(Visitor<TCode, TFiber> actual)
            => actual.ArrayGetter(this);
    }

    sealed class ArraySetter : ArrayAccess
    {
        public ArraySetter(Size elementSize, Size indexSize, string callingMethodName)
            : base(elementSize, indexSize, callingMethodName) { }

        [DisableDump]
        internal override Size InputSize => Root.DefaultRefAlignParam.RefSize * 2 + IndexSize;

        [DisableDump]
        internal override Size OutputSize => Size.Zero;

        internal override void Visit(IVisitor visitor)
            => visitor.ArraySetter(ElementSize, IndexSize);

        protected override TFiber VisitImplementation<TCode, TFiber>(Visitor<TCode, TFiber> actual)
            => actual.ArraySetter(this);
    }
}