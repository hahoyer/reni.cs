using hw.DebugFormatter;
using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory
{
    class DeclarerHandler : DumpableObject, IDeclarerProvider
    {
        Result<DeclarerSyntax> IDeclarerProvider.Get(BinaryTree target, Factory factory)
            => factory.GetDeclarationTag(target);
    }
}