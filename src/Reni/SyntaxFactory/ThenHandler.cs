using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory;

sealed class ThenHandler : DumpableObject, IValueProvider
{
    ValueSyntax IValueProvider.Get(BinaryTree target, Factory factory, Anchor anchor)
        => new CondSyntax
        (
            factory.GetValueSyntax(target.Left)!
            , factory.GetValueSyntax(target.Right)
            , null
            , Anchor.Create(target).Combine(anchor)
        );
}