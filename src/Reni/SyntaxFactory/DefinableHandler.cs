using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory;

sealed class DefinableHandler : DumpableObject, IValueProvider
{
    ValueSyntax? IValueProvider.Get(BinaryTree target, Factory factory, Anchor frameItems)
        => factory.GetExpressionSyntax(target, frameItems);
}