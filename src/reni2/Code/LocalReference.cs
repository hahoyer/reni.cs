using hw.DebugFormatter;

using Reni.Basics;
using Reni.Context;
using Reni.Type;

namespace Reni.Code
{
    sealed class LocalReference : FiberHead
    {
        static int _nextObjectId;

        public LocalReference(TypeBase valueType, CodeBase valueCode, bool isUsedOnce)
            : base(_nextObjectId++)
        {
            ValueType = valueType;
            ValueCode = valueCode;
            Tracer.Assert(valueCode.Size == ValueType.Size);
            IsUsedOnce = isUsedOnce;
            StopByObjectIds();
        }

        [Node]
        [EnableDumpExcept(false)]
        bool IsUsedOnce { get; }
        [Node]
        internal CodeBase ValueCode { get; }
        [Node]
        public TypeBase ValueType { get; }
        [DisableDump]
        internal CodeBase AlignedValueCode => ValueCode.Align();

        protected override Size GetSize() => Root.DefaultRefAlignParam.RefSize;
        protected override CodeArgs GetRefsImplementation() => ValueCode.Exts;

        protected override TCode VisitImplementation<TCode, TFiber>(Visitor<TCode, TFiber> actual)
            => actual.LocalReference(this);

        protected override CodeBase TryToCombine(FiberItem subsequentElement)
            => IsUsedOnce ? subsequentElement.TryToCombineBack(this) : null;
    }
}