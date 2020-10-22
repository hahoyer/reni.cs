using hw.DebugFormatter;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory
{
    class DeclarationMarkHandler : DumpableObject, IDeclarerProvider
    {
        DeclarerSyntax IDeclarerProvider.Get(BinaryTree target, Factory factory)
        {
            (target.Right != null).Assert();
            return
                factory.GetDeclarerSyntax(target.Left).Combine(factory.ToDeclarer(target, target.Right));
        }
    }
}