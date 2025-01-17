using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory;

sealed class AnnotationHandler : DumpableObject, IValueProvider
{
    public ValueSyntax Get(BinaryTree? target, Factory factory, Anchor frameItems)
    {
        var a = target.CheckForAnnotations();
        NotImplementedMethod(target, factory, frameItems, "a", a);
        return default!;
    }
}