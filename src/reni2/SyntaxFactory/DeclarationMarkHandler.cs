using hw.DebugFormatter;
using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory
{
    class DeclarationMarkHandler : DumpableObject, IDeclarerProvider
    {
        Result<DeclarerSyntax> IDeclarerProvider.Get(BinaryTree target, Factory factory)
        {
            (target.Right != null).Assert();

            return
                (
                    factory.GetDeclarerSyntax(target.Left),
                    factory.ToDeclarer(target, target.Right)
                )
                .Apply((left, other) => Extension.Combine(left, other));
        }
    }
}