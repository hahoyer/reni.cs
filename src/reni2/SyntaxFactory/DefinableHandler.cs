using hw.DebugFormatter;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory
{
    class DefinableHandler : DumpableObject, IValueProvider
    {
        ValueSyntax IValueProvider.Get(BinaryTree target, Factory factory, FrameItemContainer frameItems)
            => factory.GetExpressionSyntax(target, frameItems);
    }
}