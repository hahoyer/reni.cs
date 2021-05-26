using hw.DebugFormatter;
using Reni.Basics;
using Reni.SyntaxTree;
using Reni.Type;

namespace Reni.Struct
{
    sealed class GetterFunction : FunctionInstance
    {
        protected override FunctionId FunctionId { get; }

        public GetterFunction(FunctionType parent, int index, ValueSyntax body)
            : base(parent, body)
            => FunctionId = FunctionId.Getter(index);

        [DisableDump]
        internal TypeBase ReturnType => GetCallResult(Category.Type).Type;

        [DisableDump]
        protected override Size RelevantValueSize => Size.Zero;
    }
}