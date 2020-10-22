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
            Factory factory, FrameItemContainer brackets
        )
        {
            var bracketKernel = target.BracketKernel;

            return bracketKernel
                .Apply(kernel =>
                {
                    var result = factory.GetValueSyntax(kernel.Left, kernel.Center, kernel.Right);
                    result.Target.AssertIsNotNull();
                    return result;
                });
        }

    }
}