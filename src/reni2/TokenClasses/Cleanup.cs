using hw.Parser;
using Reni.Parser;
using Reni.Struct;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class Cleanup : TokenClass, IValueProvider, SyntaxFactory.IValueToken
    {
        public const string TokenId = "~~~";
        public override string Id => TokenId;

        SyntaxFactory.IValueProvider SyntaxFactory.IValueToken.Provider => SyntaxFactory.Cleanup;

        Result<ValueSyntax> IValueProvider.Get(BinaryTree binaryTree, ISyntaxScope scope)
        {
            var statements = binaryTree.Left?.GetStatements(scope);
            if(statements == null)
                statements = new Statement[0];
            var cleanup = binaryTree.Right?.Syntax(scope);
            return CompoundSyntax.Create(statements, cleanup, binaryTree);
        }
    }
}