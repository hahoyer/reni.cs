using hw.DebugFormatter;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory
{
    class ElseHandler : DumpableObject, IValueProvider
    {
        ValueSyntax IValueProvider.Get(BinaryTree target, Factory factory, FrameItemContainer frameItems)
            => new CondSyntax
            (
                factory.GetValueSyntax(target.Left?.Left)
                , factory.GetValueSyntax(target.Left?.Right)
                , factory.GetValueSyntax(target.Right)
                , frameItems
            );
    }
}