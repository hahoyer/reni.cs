using hw.Parser;
using Reni.Parser;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class ThenToken : InfixToken, IValueProvider
    {
        public const string TokenId = "then";
        public override string Id => TokenId;

        Result<Syntax> IValueProvider.Get(BinaryTree binaryTree, IValuesScope scope)
            => CondSyntax.Create(binaryTree.Left, binaryTree.Right, null, binaryTree, scope);
    }
}