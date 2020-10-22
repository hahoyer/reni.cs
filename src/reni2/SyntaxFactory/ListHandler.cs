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
            (BinaryTree leftAnchor, BinaryTree target, Factory factory, FrameItemContainer brackets)
        {
            var rightFrameItems = FrameItemContainer.Create(target);



            return (
                    factory.GetStatementsSyntax(leftAnchor, target.Left, brackets),
                    factory.GetStatementsSyntax(target, target.Right, rightFrameItems)
                )
                .Apply((left, right) => left.Concat(right).ToArray());
        }
    }
}