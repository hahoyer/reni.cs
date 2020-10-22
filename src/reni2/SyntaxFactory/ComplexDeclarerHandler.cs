using hw.DebugFormatter;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory
{
    class ComplexDeclarerHandler : DumpableObject, IDeclarerProvider
    {
        DeclarerSyntax IDeclarerProvider.Get(BinaryTree target, Factory factory)
        {
            NotImplementedMethod(target, factory);
            return factory.GetDeclarationTags(target.BracketKernel);
        }
    }
}