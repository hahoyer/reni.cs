using System.Reflection;
using Reni.Basics;

namespace Reni.Code;

class ForeignCall : FiberItem
{
    readonly MethodInfo MethodInfo;
    readonly Size ResultSize;
    readonly Size ArgsSize;

    internal ForeignCall(MethodInfo methodInfo, Size resultSize, Size argsSize)
    {
        MethodInfo = methodInfo;
        ResultSize = resultSize;
        ArgsSize = argsSize;
    }

    internal override Size InputSize => ArgsSize;

    internal override Size OutputSize => ResultSize;

    internal override void Visit(IVisitor visitor) => visitor.ForeignCall(OutputSize, MethodInfo, InputSize);
}
