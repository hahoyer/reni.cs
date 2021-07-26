using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using Reni.Helper;
using Reni.Parser;
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
            var name = target.Left;
            var exclamation = target.Left;

            if(target.Left.TokenClass is not ExclamationBoxToken)
                exclamation = exclamation.Left;
            else
                name = null;

            var tags = GetDeclarationTags(exclamation);
            var declarer
                = DeclarerSyntax
                    .Create(tags, name, factory.MeansPublic, target.Left);

            var result = DeclarationSyntax
                .Create(declarer, factory.GetValueSyntax(target.Right), Anchor.Create(target));
            return result;
        }

        static(BinaryTree[] Anchors, BinaryTree tag)[] GetDeclarationTags(BinaryTree target)
            => target
                .Chain(node => node.Left)
                .SelectMany(GetDeclarationTag)
                .ToArray();

        static(BinaryTree[] anchors, BinaryTree tag)[] GetDeclarationTag(BinaryTree target)
        {
            target.AssertIsNotNull();

            if(target.TokenClass is not ExclamationBoxToken)
                return T((anchors: T(target.Right), tag: target));

            target.Right.AssertIsNotNull();

            var nodes = target
                .Right
                .GetNodesFromLeftToRight()
                .GroupBy(node => node.TokenClass is IDeclarationTagToken)
                .ToDictionary(group => group.Key, group => group.ToArray());
            var tags = nodes.SingleOrDefault(node => node.Key).Value;
            var result = tags.Select(tag => (anchors: new BinaryTree[0], tag)).ToArray();
            result[0].anchors
                = T(T(target), nodes.SingleOrDefault(node => !node.Key).Value)
                    .ConcatMany()
                    .ToArray();
            return result;
        }

        static Kind Classification(BinaryTree node)
            => node.TokenClass switch
            {
                Definable => Kind.Name, IDeclarationTagToken => Kind.Tag, _ => Kind.Anchor
            };
    }
}