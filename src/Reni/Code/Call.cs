using Reni.Basics;
using Reni.Struct;

namespace Reni.Code;

sealed class Call : FiberItem
{
    [Node]
    [DisableDump]
    internal readonly FunctionId FunctionId;

    [Node]
    [DisableDump]
    internal readonly Size ArgsAndRefsSize;

    [Node]
    [DisableDump]
    readonly Size ResultSize;

    internal Call(FunctionId functionId, Size resultSize, Size argsAndRefsSize)
    {
        FunctionId = functionId;
        ResultSize = resultSize;
        ArgsAndRefsSize = argsAndRefsSize;
        StopByObjectIds(-1);
    }

    [DisableDump]
    internal override Size InputSize => ArgsAndRefsSize;

    [DisableDump]
    internal override Size OutputSize => ResultSize;

    protected override TFiber? VisitImplementation<TCode, TFiber>(Visitor<TCode, TFiber> actual)
        where TFiber : default
        => actual.Call(this);

    protected override string GetNodeDump()
        => base.GetNodeDump() + " FunctionId=" + FunctionId + " ArgsAndRefsSize=" + ArgsAndRefsSize;

    internal override void Visit(IVisitor visitor) => visitor.Call(OutputSize, FunctionId, ArgsAndRefsSize);

    public FiberItem TryConvertToRecursiveCall(FunctionId functionId)
    {
        if(FunctionId != functionId)
            return this;
        ResultSize.IsZero.Assert();
        return ArgsAndRefsSize.GetRecursiveCall();
    }
}
