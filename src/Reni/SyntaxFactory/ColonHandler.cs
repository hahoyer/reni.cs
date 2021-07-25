using System.Linq;
using hw.DebugFormatter;
using NUnit.Framework;
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
            (name.TokenClass is Definable).Assert();

            var tagCandidates = target.Left.Left;
            (tagCandidates.TokenClass is ExclamationBoxToken).Assert();
            var tags = T((tagCandidates, tagCandidates.Right));
            var declarer = DeclarerSyntax.Create(tags, name, factory.MeansPublic, target.Left);

            return DeclarationSyntax.Create(declarer, factory.GetValueSyntax(target.Right), Anchor.Create(target));
        }

        static Kind Classification(BinaryTree node)
            => node.TokenClass switch
            {
                Definable => Kind.Name, IDeclarationTagToken => Kind.Tag, _ => Kind.Anchor
            };
    }
}