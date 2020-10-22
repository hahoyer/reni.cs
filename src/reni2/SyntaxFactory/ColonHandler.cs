using hw.DebugFormatter;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory
{
    class ColonHandler : DumpableObject, IStatementProvider
    {
        IStatementSyntax IStatementProvider.Get(BinaryTree target, Factory factory)
            => DeclarationSyntax
                .Create(factory.GetDeclarerSyntax(target.Left), target, factory.GetValueSyntax(target.Right));
    }
}