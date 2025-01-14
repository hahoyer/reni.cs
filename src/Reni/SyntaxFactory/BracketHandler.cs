using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory;

sealed class BracketHandler : DumpableObject, IValueProvider
{
    ValueSyntax? IValueProvider.Get(BinaryTree? target, Factory factory, Anchor anchor)
    {
        var kernel = target.BracketKernel;
        var result = factory.GetValueSyntax(kernel.Center, kernel.ToAnchor.Combine(anchor));
        result.AssertIsNotNull();
        return result;
    }
}