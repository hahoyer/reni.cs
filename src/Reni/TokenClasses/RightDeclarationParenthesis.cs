using hw.Parser;
using Reni.Parser;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(DeclarationTokenFactory))]
    sealed class RightDeclarationParenthesis
        : RightParenthesisBase
            , IBracketMatch<BinaryTree>

    {
        sealed class Matched
            : DumpableObject
                , IParserTokenType<BinaryTree>
        {
            BinaryTree IParserTokenType<BinaryTree>.Create(BinaryTree left, IToken token, BinaryTree right)
            {
                (right == null).Assert();
                return left;
            }

            string IParserTokenType<BinaryTree>.PrioTableId => "()";
        }

        public RightDeclarationParenthesis(int level)
            : base(level) { }

        IParserTokenType<BinaryTree> IBracketMatch<BinaryTree>.Value { get; } = new Matched();
    }
}