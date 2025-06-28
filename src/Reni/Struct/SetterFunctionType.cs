using Reni.Basics;
using Reni.Context;
using Reni.SyntaxTree;
using Reni.Type;

namespace Reni.Struct;

sealed class SetterFunction : FunctionInstance
{
    protected override FunctionId FunctionId { get; }

    public SetterFunction(FunctionType parent, int index, ValueSyntax body)
        : base(parent, body) 
        => FunctionId = FunctionId.Setter(index);

    [DisableDump]
    protected override TypeBase CallType => base.CallType.GetPair(Parent.ValueType.Pointer);
    [DisableDump]
    protected override Size RelevantValueSize => Root.DefaultRefAlignParam.RefSize;
}