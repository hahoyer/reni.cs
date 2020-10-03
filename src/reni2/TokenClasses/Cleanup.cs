using hw.Parser;
using Reni.Parser;
using Reni.Struct;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class Cleanup : TokenClass, IValueProvider
    {
        public const string TokenId = "~~~";
        public override string Id => TokenId;

        Result<Syntax> IValueProvider.Get(BinaryTree binaryTree, IValuesScope scope)
        {
            var statements = binaryTree.Left?.GetStatements(scope);
            if(statements == null)
                statements = new Statement[0];
            var cleanup = binaryTree.Right?.Value(scope);
            return CompoundSyntax.Create(statements, cleanup, binaryTree);
        }
    }
}