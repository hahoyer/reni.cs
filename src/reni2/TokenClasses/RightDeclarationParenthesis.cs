using hw.DebugFormatter;
using hw.Parser;
using Reni.Parser;
using Reni.Validation;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(DeclarationTokenFactory))]
    sealed class RightDeclarationParenthesis
        : RightParenthesisBase
            , IDeclarerTagProvider
            , IBracketMatch<BinaryTree>,SyntaxFactory.IDeclarerToken

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

        static Result<IDeclarationTag> GetDeclarationTag(BinaryTree item)
        {
            if(item.TokenClass is IDeclarationTag result)
                return new Result<IDeclarationTag>(result);

            return new Result<IDeclarationTag>(null, IssueId.InvalidDeclarationTag.Issue(item.SourcePart));
        }

        SyntaxFactory.IDeclarerProvider SyntaxFactory.IDeclarerToken.Provider => SyntaxFactory.ComplexDeclarer;
    }
}