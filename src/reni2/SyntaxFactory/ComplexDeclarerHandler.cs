using hw.DebugFormatter;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory
{
    class ComplexDeclarerHandler : DumpableObject, IDeclarerProvider
    {
        DeclarerSyntax IDeclarerProvider.Get(BinaryTree target, Factory factory)
            => factory.GetDeclarationTags(target.BracketKernel);
    }
}