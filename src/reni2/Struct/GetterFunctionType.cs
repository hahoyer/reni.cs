using hw.DebugFormatter;
using Reni.Basics;
using Reni.Parser;
using Reni.Type;

namespace Reni.Struct
{
    sealed class GetterFunction : FunctionInstance
    {
        public GetterFunction(FunctionType parent, int index, Value body)
            : base(parent, body) { FunctionId = FunctionId.Getter(index); }

        [DisableDump]
        internal TypeBase ReturnType => GetCallResult(Category.Type).Type;
        [DisableDump]
        protected override Size RelevantValueSize => Size.Zero;
        protected override FunctionId FunctionId { get; }
    }
}