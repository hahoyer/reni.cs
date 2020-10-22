using hw.DebugFormatter;
using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory
{
    class MatchedBracketHandler : DumpableObject, IValueProvider
    {
        ValueSyntax IValueProvider.Get(BinaryTree target, Factory factory, FrameItemContainer frameItems)
            => new ExpressionSyntax(target, factory.GetValueSyntax(target.Left), null
                , factory.GetValueSyntax(target.Right), frameItems);
    }
}