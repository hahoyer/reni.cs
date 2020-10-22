using hw.DebugFormatter;
using hw.Parser;
using Reni.Parser;
using Reni.SyntaxFactory;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(DeclarationTokenFactory))]
    sealed class RightDeclarationParenthesis
        : RightParenthesisBase
            , IBracketMatch<BinaryTree>,IDeclarationTagToken

    {
        sealed class Matched
            : DumpableObject
                , IParserTokenType<BinaryTree>
        {
            BinaryTree IParserTokenType<BinaryTree>.Create(BinaryTree left, IToken token, BinaryTree right)
            {
                Tracer.Assert(right == null);
                return left;
            }

            string IParserTokenType<BinaryTree>.PrioTableId => "()";
        }

        public RightDeclarationParenthesis(int level)
            : base(level) { }

        IParserTokenType<BinaryTree> IBracketMatch<BinaryTree>.Value { get; } = new Matched();

        IDeclarerProvider IDeclarationTagToken.Provider => Factory.ComplexDeclarer;
    }
}