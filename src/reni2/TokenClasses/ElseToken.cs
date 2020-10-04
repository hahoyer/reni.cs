using hw.DebugFormatter;
using hw.Parser;
using Reni.Parser;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class ElseToken : TokenClass, IValueProvider
    {
        public const string TokenId = "else";
        public override string Id => TokenId;

        Result<ValueSyntax> IValueProvider.Get(BinaryTree binaryTree, ISyntaxScope scope)
        {
            Tracer.Assert(binaryTree.Left != null);
            Tracer.Assert(binaryTree.Left.TokenClass is ThenToken);
            return CondSyntax.Create(binaryTree.Left.Left, binaryTree.Left.Right, binaryTree.Right, binaryTree, scope);
        }
    }
}