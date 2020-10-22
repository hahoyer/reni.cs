using hw.DebugFormatter;
using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory
{
    class BracketHandler : DumpableObject, IValueProvider
    {
        ValueSyntax IValueProvider.Get(BinaryTree target, Factory factory, FrameItemContainer brackets)
        {
            var kernel = target.BracketKernel;
            var result = factory.GetValueSyntax(kernel.Center, FrameItemContainer.Create(kernel.Left, kernel.Right));
            result.AssertIsNotNull();
            return result;
        }
    }
}