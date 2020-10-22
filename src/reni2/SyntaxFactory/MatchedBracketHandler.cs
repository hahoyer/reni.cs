using hw.DebugFormatter;
using Reni.Helper;
using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory
{
    class MatchedBracketHandler : DumpableObject, IValueProvider
    {
        Result<ValueSyntax> IValueProvider.Get
            (BinaryTree leftAnchor, BinaryTree target, BinaryTree rightAnchor, Factory factory)
            => (
                    factory.GetValueSyntax(target.Left),
                    factory.GetValueSyntax(target.Right)
                )
                .Apply((left, right)
                    => (ValueSyntax)new ExpressionSyntax(target, left, null, right));

        UsageTree IValueProvider.GetUsage
            (BinaryTree leftAnchor, BinaryTree target, Factory factory)
            => new UsageTree {};
    }
}