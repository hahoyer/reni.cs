using System.Linq;
using hw.DebugFormatter;
using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory
{
    class ListHandler : DumpableObject, IStatementsProvider
    {
        Result<IStatementSyntax[]> IStatementsProvider.Get
            (BinaryTree target, Factory factory, FrameItemContainer brackets)
            => (
                    factory.GetStatementsSyntax(target.Left, brackets),
                    factory.GetStatementsSyntax(target.Right, FrameItemContainer.Create(target))
                )
                .Apply((left, right) => left.Concat(right).ToArray());
    }
}