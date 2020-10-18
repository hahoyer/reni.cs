using hw.DebugFormatter;
using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.SyntaxFactory
{
    class InvalidTokenDeclarerHandler : DumpableObject, IDeclarerProvider
    {
        Result<DeclarerSyntax> IDeclarerProvider.Get(BinaryTree target, Factory factory)
        {
            target.Right.AssertIsNull();

            return factory
                .ToDeclarer(factory.GetDeclarerSyntax(target.Left), target, target.Token.Characters.Id)
                .With(IssueId.InvalidNameForDeclaration.Issue(target.Token.Characters));
        }
    }
}