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
            if(target.Left == null && target.Right == null)
                return factory.GetDeclarationName(target);

            if(target.Left != null && target.Right == null)
            {
                var left = factory.GetDeclarerSyntax(target.Left);
                var center = factory.GetDeclarationName(target);
                return left.Combine(center);
            }

            
            NotImplementedMethod(target, factory);
            target.Right.AssertIsNull();

            return factory.ToDeclarer(factory.GetDeclarerSyntax(target.Left), target, target.Token.Characters.Id);
        }

        ValueSyntax IValueProvider.Get(BinaryTree target, Factory factory, FrameItemContainer frameItems)
            => factory.GetExpressionSyntax(target, frameItems);
    }
}