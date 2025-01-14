using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory;

sealed class ElseHandler : DumpableObject, IValueProvider
{
    ValueSyntax? IValueProvider.Get(BinaryTree? target, Factory factory, Anchor frameItems)
    {
        var condTarget = target.Left;
        BinaryTree? thenTarget = null;
        BinaryTree thenAnchor = null;

        if(condTarget?.TokenClass is ThenToken)
        {
            thenAnchor = condTarget;
            thenTarget = thenAnchor.Right;
            condTarget = thenAnchor.Left;
        }

        return new CondSyntax
        (
            factory.GetValueSyntax(condTarget)
            , factory.GetValueSyntax(thenTarget)
            , factory.GetValueSyntax(target.Right)
            , Anchor.Create(thenAnchor, target).Combine(frameItems)
        );
    }
}