using hw.DebugFormatter;
using hw.Parser;
using Reni.Parser;

namespace Reni.TokenClasses
{
    sealed class EndOfText
        : TokenClass
            , IDefaultScopeProvider
            , IBracketMatch<BinaryTree>
            , ISyntaxScope
            , IBelongingsMatcher
            , SyntaxFactory.IValueToken
            , IRightBracket

    {
        sealed class Matched : DumpableObject, IParserTokenType<BinaryTree>
        {
            BinaryTree IParserTokenType<BinaryTree>.Create(BinaryTree left, IToken token, BinaryTree right)
            {
                Tracer.Assert(right == null);
                Tracer.Assert(left.Right == null);
                Tracer.Assert(left.Left.Left == null);
                return left;
            }

            string IParserTokenType<BinaryTree>.PrioTableId => "()";
        }

        const string TokenId = PrioTable.EndOfText;

        [DisableDump]
        public override string Id => TokenId;

        bool IBelongingsMatcher.IsBelongingTo(IBelongingsMatcher otherMatcher)
            => otherMatcher is BeginOfText;

        IParserTokenType<BinaryTree> IBracketMatch<BinaryTree>.Value { get; } = new Matched();
        bool IDefaultScopeProvider.MeansPublic => true;

        int IRightBracket.Level => -1;

        IDefaultScopeProvider ISyntaxScope.DefaultScopeProvider => this;

        bool ISyntaxScope.IsDeclarationPart => false;

        SyntaxFactory.IValueProvider SyntaxFactory.IValueToken.Provider => SyntaxFactory.Bracket;
    }

    sealed class BeginOfText : TokenClass, IBelongingsMatcher, ILeftBracket
    {
        const string TokenId = PrioTable.BeginOfText;

        [DisableDump]
        public override string Id => TokenId;

        bool IBelongingsMatcher.IsBelongingTo(IBelongingsMatcher otherMatcher)
            => otherMatcher is EndOfText;

        int ILeftBracket.Level => -1;
    }
}