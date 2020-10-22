using hw.DebugFormatter;
using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory
{
    class DefinableHandler : DumpableObject, IDeclarerProvider, IValueProvider
    {
        Result<DeclarerSyntax> IDeclarerProvider.Get(BinaryTree target, Factory factory)
        {
            target.Right.AssertIsNull();

            return factory.ToDeclarer(factory.GetDeclarerSyntax(target.Left), target, target.Token.Characters.Id);
        }

        Result<ValueSyntax> IValueProvider.Get
            (BinaryTree leftAnchor, BinaryTree target, BinaryTree rightAnchor, Factory factory)
            => Result<ValueSyntax>.From(factory.GetExpressionSyntax(target));

        UsageTree IValueProvider.GetUsage
            (BinaryTree leftAnchor, BinaryTree target, Factory factory)
            => new UsageTree {};
    }
}