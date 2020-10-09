using hw.DebugFormatter;
using hw.Parser;
using Reni.Parser;

namespace Reni.TokenClasses
{
    sealed class EndOfText
        : TokenClass
            , IDefaultScopeProvider
            , IBracketMatch<BinaryTree>
            , IStatementsProvider
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

        int IRightBracket.Level => -1;

        bool IBelongingsMatcher.IsBelongingTo(IBelongingsMatcher otherMatcher)
            => otherMatcher is BeginOfText;

        IParserTokenType<BinaryTree> IBracketMatch<BinaryTree>.Value { get; } = new Matched();
        bool IDefaultScopeProvider.MeansPublic => true;

        SyntaxFactory.IValueProvider SyntaxFactory.IValueToken.Provider => SyntaxFactory.Bracket;

        IDefaultScopeProvider ISyntaxScope.DefaultScopeProvider => this;

        bool ISyntaxScope.IsDeclarationPart => false;
    }

    sealed class BeginOfText : TokenClass, IBelongingsMatcher, ILeftBracket
    {
        const string TokenId = PrioTable.BeginOfText;

        [DisableDump]
        public override string Id => TokenId;

        int ILeftBracket.Level => -1;
        bool IBelongingsMatcher.IsBelongingTo(IBelongingsMatcher otherMatcher)
            => otherMatcher is EndOfText;
    }

}