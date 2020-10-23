using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using Reni.Helper;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory
{
    class ColonHandler : DumpableObject, IStatementProvider
    {
        IStatementSyntax IStatementProvider.Get(BinaryTree target, Factory factory)
        {
            var nodes = target
                .Left
                .GetNodesFromLeftToRight()
                .Where(node => node != null)
                .Split(node => node.TokenClass is IDeclarationTagToken, true)
                .ToArray();

            var hasPrefix = !(nodes.First().First().TokenClass is IDeclarationTagToken);
            var frameItems = hasPrefix? FrameItemContainer.Create(nodes.First()) : null;

            var result = nodes
                .Skip(hasPrefix?1:0)
                .Select(factory.CombineWithSuffix)
                .Aggregate();

            return DeclarationSyntax
                .Create(result, target, factory.GetValueSyntax(target.Right), frameItems);
        }
    }
}