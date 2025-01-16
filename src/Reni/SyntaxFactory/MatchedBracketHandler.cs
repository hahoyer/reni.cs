using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory;

sealed class MatchedBracketHandler : DumpableObject, IValueProvider
{
    ValueSyntax IValueProvider.Get(BinaryTree target, Factory factory, Anchor? anchor)
        => new ExpressionSyntax
        (
            factory.GetValueSyntax(target.Left)
            , null
            , target.Token
            , factory.GetValueSyntax(target.Right)
            , Anchor.Create(target).Combine(anchor)
        );
}