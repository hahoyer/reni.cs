using hw.DebugFormatter;
using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory
{
    class ThenHandler : DumpableObject, IValueProvider
    {
        ValueSyntax IValueProvider.Get(BinaryTree target, Factory factory, FrameItemContainer frameItems)
            => new CondSyntax(factory.GetValueSyntax(target.Left), factory.GetValueSyntax(target.Right), null, target
                , frameItems);
    }
}