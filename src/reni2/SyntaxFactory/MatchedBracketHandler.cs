using hw.DebugFormatter;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory
{
    class MatchedBracketHandler : DumpableObject, IValueProvider
    {
        ValueSyntax IValueProvider.Get(BinaryTree target, Factory factory, Anchor frameItems)
            => new ExpressionSyntax(factory.GetValueSyntax(target.Left), null
                , factory.GetValueSyntax(target.Right), frameItems);
    }
}