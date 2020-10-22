using hw.DebugFormatter;
using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.SyntaxFactory
{
    class InvalidTokenDeclarerHandler : DumpableObject, IDeclarerProvider
    {
        DeclarerSyntax IDeclarerProvider.Get(BinaryTree target, Factory factory)
        {
            NotImplementedMethod(target, factory);
            target.Right.AssertIsNull();

            return factory
                .ToDeclarer
                (
                    factory.GetDeclarerSyntax(target.Left),
                    target,
                    target.Token.Characters.Id,
                    IssueId.InvalidNameForDeclaration.Issue(target.Token.Characters)
                );
        }
    }
}