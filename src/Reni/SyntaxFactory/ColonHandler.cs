using hw.DebugFormatter;
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

            if(target.Left.TokenClass is Definable)
                exclamation = exclamation.Left;
            else
                name = null;

            var tags = GetDeclarationTags(exclamation);
            var declarer = DeclarerSyntax.Create(tags, name, factory.MeansPublic, target.Left);

            return DeclarationSyntax.Create(declarer, factory.GetValueSyntax(target.Right), Anchor.Create(target));
        }

        static(BinaryTree tagCandidates, BinaryTree Right)[] GetDeclarationTags(BinaryTree exclamation)
        {
            if(exclamation == null)
                return null;
            exclamation.TokenClass.Assert<ExclamationBoxToken>();
            exclamation.Left.AssertIsNull();
            exclamation.Right.AssertIsNotNull();
            exclamation.Right.TokenClass.Assert<DeclarationTagToken>();
            return T((tagCandidates: exclamation, exclamation.Right));
        }

        static Kind Classification(BinaryTree node)
            => node.TokenClass switch
            {
                Definable => Kind.Name, IDeclarationTagToken => Kind.Tag, _ => Kind.Anchor
            };
    }
}