using hw.DebugFormatter;
using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory
{
    class ComplexDeclarerHandler : DumpableObject, IDeclarerProvider
    {
        Result<DeclarerSyntax> IDeclarerProvider.Get(BinaryTree target, Factory factory)
            => target.BracketKernel
                .Apply(factory.GetDeclarationTags);
    }
}