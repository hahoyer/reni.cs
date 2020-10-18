using hw.DebugFormatter;
using hw.Parser;
using Reni.Parser;
using Reni.SyntaxFactory;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class RightParenthesis
        : RightParenthesisBase
            , IBracketMatch<BinaryTree>
            , ISyntaxScope
            , IValueToken
    {
        sealed class Matched
            : DumpableObject, IParserTokenType<BinaryTree>, ITokenClass, IValueToken
        {
            static string Id => "()";

            BinaryTree IParserTokenType<BinaryTree>.Create(BinaryTree left, IToken token, BinaryTree right)
                => right == null? left : BinaryTree.Create(left, this, token, right);

            string IParserTokenType<BinaryTree>.PrioTableId => Id;
            string ITokenClass.Id => Id;

            IValueProvider IValueToken.Provider => Factory.MatchedBracket;
        }

        public RightParenthesis(int level)
            : base(level) { }

        [EnableDump]
        new int Level => base.Level;

        IParserTokenType<BinaryTree> IBracketMatch<BinaryTree>.Value { get; } = new Matched();

        IValueProvider IValueToken.Provider => Factory.Bracket;
    }
}