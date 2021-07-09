using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using Reni.Helper;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory
{
    class ColonHandler : DumpableObject, IStatementProvider
    {
        enum Kind
        {
            Anchor
            , Tag
            , Name
        }

        IStatementSyntax IStatementProvider.Get(BinaryTree target, Factory factory)
        {
            var nodes = target
                .Left
                .GetNodesFromLeftToRight()
                .Where(node => node != null)
                .GroupBy(Classification)
                .ToDictionary(pair => pair.Key, pair => pair.ToArray());

            nodes.TryGetValue(Kind.Tag, out var tags);
            nodes.TryGetValue(Kind.Name, out var names);

            var declarer = DeclarerSyntax.Create(tags, names, factory.MeansPublic, target.Left);

            return DeclarationSyntax.Create(declarer, factory.GetValueSyntax(target.Right), Anchor.Create(target));
        }

        static Kind Classification(BinaryTree node)
            => node.TokenClass switch
            {
                Definable => Kind.Name, IDeclarationTagToken => Kind.Tag, _ => Kind.Anchor
            };
    }
}