using Reni.Basics;
using Reni.Context;
using Reni.Parser;
using Reni.Type;

namespace Reni.Struct
{
    sealed class SetterFunction : FunctionInstance
    {
        public SetterFunction(FunctionType parent, int index, Syntax body)
            : base(parent, body) => FunctionId = FunctionId
            .Setter(index);

        protected override FunctionId FunctionId { get; }

        protected override TypeBase CallType => base.CallType.Pair(Parent.ValueType.Pointer, null);
        protected override Size RelevantValueSize => Root.DefaultRefAlignParam.RefSize;
    }
}