using hw.DebugFormatter;
using Reni.Parser;
using Reni.Struct;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory
{
    class ColonHandler : DumpableObject, IStatementProvider
    {
        Result<IStatementSyntax> IStatementProvider.Get(BinaryTree target, Factory factory)
            =>
                (
                    factory.GetDeclarerSyntax(target.Left),
                    factory.GetValueSyntax(target.Right)
                )
                .Apply
                ((declarer, value) => DeclarationSyntax.Create(declarer, target, value)
                );
    }
}