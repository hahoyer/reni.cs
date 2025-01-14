using Reni.Code;
using Reni.Code.ReplaceVisitor;
using Reni.Struct;

namespace Reni.Context;

sealed class ReplacePrimitiveRecursivity : Base
{
    static int NextObjectId;

    [EnableDump]
    readonly FunctionId FunctionId;

    public ReplacePrimitiveRecursivity(FunctionId functionId)
        : base(NextObjectId++)
        => FunctionId = functionId;

    internal override CodeBase? List(List visitedObject)
    {
        var visitor = this;
        var data = visitedObject.Data;
        var newList = new CodeBase?[data.Length];
        var index = data.Length - 1;
        var codeBase = data[index];
        newList[index] = codeBase.Visit(visitor);
        return visitor.List(visitedObject, newList);
    }

    internal override CodeBase? Fiber(Fiber visitedObject)
    {
        var data = visitedObject.FiberItems;
        var newItems = new FiberItem?[data.Length];
        var index = data.Length - 1;
        newItems[index] = data[index].Visit(this);
        return Fiber(visitedObject, null, newItems);
    }

    internal override FiberItem Call(Call visitedObject) => visitedObject.TryConvertToRecursiveCall(FunctionId);
}