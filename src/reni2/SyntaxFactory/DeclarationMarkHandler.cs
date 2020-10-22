using hw.DebugFormatter;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory
{
    class DeclarationMarkHandler : DumpableObject, IDeclarerProvider
    {
        DeclarerSyntax IDeclarerProvider.Get(BinaryTree target, Factory factory)
        {
            if(target.Left == null && target.Right != null)
                return factory.GetDeclarationTags(target.Right, FrameItemContainer.Create(target));


            NotImplementedMethod(target, factory);
            return default;
        }
    }
}