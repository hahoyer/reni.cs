using hw.DebugFormatter;
using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory
{
    class DefinableHandler : DumpableObject, IDeclarerProvider, IValueProvider
    {
        DeclarerSyntax IDeclarerProvider.Get(BinaryTree target, Factory factory)
        {
            target.Right.AssertIsNull();
            return factory.ToDeclarer(factory.GetDeclarerSyntax(target.Left), target, target.Token.Characters.Id);
        }

        ValueSyntax IValueProvider.Get(BinaryTree target, Factory factory, FrameItemContainer frameItems)
            => factory.GetExpressionSyntax(target, frameItems);
    }
}