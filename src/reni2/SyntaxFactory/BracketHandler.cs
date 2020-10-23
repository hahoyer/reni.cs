using hw.DebugFormatter;
using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory
{
    class BracketHandler : DumpableObject, IValueProvider
    {
        ValueSyntax IValueProvider.Get(BinaryTree target, Factory factory, FrameItemContainer frameItems)
        {
            var kernel = target.BracketKernel;
            var frameItemContainer = kernel.ToFrameItems.Combine(frameItems);
            var result = factory.GetValueSyntax(kernel.Center, frameItemContainer);
            result.AssertIsNotNull();
            return result;
        }
    }
}