using hw.DebugFormatter;
using hw.Parser;
using Reni.Parser;

namespace Reni.TokenClasses
{
    abstract class TokenClass : ParserTokenType<BinaryTree>, ITokenClass
    {
        string ITokenClass.Id => Id;

        protected override BinaryTree Create(BinaryTree left, IToken token, BinaryTree right)
            => BinaryTree.Create(left, this, token, right);

        [DisableDump]
        internal virtual bool IsVisible => true;
    }
}