using hw.DebugFormatter;

using Reni.Basics;
using Reni.Context;
using Reni.Type;

namespace Reni.Code
{
    sealed class LocalReference : FiberHead
    {
        static int NextObjectId;

        public LocalReference(TypeBase valueType, CodeBase valueCode, bool isUsedOnce)
            : base(NextObjectId++)
        {
            ValueType = valueType;
            ValueCode = valueCode;
            (valueCode.Size == ValueType.Size).Assert();
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
        internal CodeBase AlignedValueCode => ValueCode.GetAlign();

        protected override Size GetSize() => Root.DefaultRefAlignParam.RefSize;
        protected override Closures GetRefsImplementation() => ValueCode.Closures;

        protected override TCode VisitImplementation<TCode, TFiber>(Visitor<TCode, TFiber> actual)
            => actual.LocalReference(this);

        protected override CodeBase TryToCombine(FiberItem subsequentElement)
            => IsUsedOnce ? subsequentElement.TryToCombineBack(this) : null;
    }
}