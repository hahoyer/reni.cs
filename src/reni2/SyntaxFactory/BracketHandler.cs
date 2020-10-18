using hw.DebugFormatter;
using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory
{
    class BracketHandler : DumpableObject, IValueProvider
    {
        Result<ValueSyntax> IValueProvider.Get
        (
            BinaryTree leftAnchor,
            BinaryTree target,
            BinaryTree rightAnchor,
            Factory factory
        )
            => target
                .GetBracketKernel()
                .Apply(kernel =>
                {
                    var result = factory.GetValueSyntax(target.Left, kernel, target, target.EmptyListIfNull);
                    result.Target.AssertIsNotNull();
                    return result;
                });
    }
}