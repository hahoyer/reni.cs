using hw.DebugFormatter;
using hw.Parser;
using Reni.Parser;
using Reni.SyntaxFactory;

namespace Reni.TokenClasses
{
    sealed class EndOfText
        : TokenClass
            , IBracketMatch<BinaryTree>
            , ISyntaxScope
            , IBelongingsMatcher
            , IRightBracket
            , IValueToken

    {
        sealed class Matched : DumpableObject, IParserTokenType<BinaryTree>
        {
            BinaryTree IParserTokenType<BinaryTree>.Create(BinaryTree left, IToken token, BinaryTree right)
            {
                (right == null).Assert();
                (left.Right == null).Assert();
                (left.Left.Left == null).Assert();
                return left;
            }

            string IParserTokenType<BinaryTree>.PrioTableId => "()";
        }

        const string TokenId = PrioTable.EndOfText;
        public static readonly EndOfText Instance = new();

        EndOfText() { }

        bool IBelongingsMatcher.IsBelongingTo(IBelongingsMatcher otherMatcher)
            => otherMatcher is BeginOfText;

        IParserTokenType<BinaryTree> IBracketMatch<BinaryTree>.Value { get; } = new Matched();
        int IRightBracket.Level => 0;

        IValueProvider IValueToken.Provider => Factory.Bracket;

        [DisableDump]
        public override string Id => TokenId;
    }

    sealed class EndOfSubText
        : TokenClass
            , ISyntaxScope

    {
        const string TokenId = PrioTable.EndOfText;
        public static readonly EndOfSubText Instance = new();

        EndOfSubText() { }

        [DisableDump]
        public override string Id => TokenId;
    }

    sealed class BeginOfText : TokenClass, IBelongingsMatcher, ILeftBracket
    {
        const string TokenId = PrioTable.BeginOfText;
        public static readonly BeginOfText Instance = new();

        bool IBelongingsMatcher.IsBelongingTo(IBelongingsMatcher otherMatcher)
            => otherMatcher is EndOfText;

        int ILeftBracket.Level => 0;

        [DisableDump]
        public override string Id => TokenId;
    }
}