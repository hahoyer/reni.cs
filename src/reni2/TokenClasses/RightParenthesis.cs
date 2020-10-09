using hw.DebugFormatter;
using hw.Parser;
using Reni.Parser;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class RightParenthesis
        : RightParenthesisBase
            , IDefaultScopeProvider
            , IBracketMatch<BinaryTree>
            , ISyntaxScope
            , SyntaxFactory.IValueToken
    {
        sealed class Matched
            : DumpableObject, IParserTokenType<BinaryTree>, ITokenClass, SyntaxFactory.IValueToken
        {
            static string Id => "()";

            BinaryTree IParserTokenType<BinaryTree>.Create(BinaryTree left, IToken token, BinaryTree right)
                => right == null? left : BinaryTree.Create(left, this, token, right);

            string IParserTokenType<BinaryTree>.PrioTableId => Id;
            string ITokenClass.Id => Id;

            SyntaxFactory.IValueProvider SyntaxFactory.IValueToken.Provider => SyntaxFactory.MatchedBracket;
        }

        public RightParenthesis(int level)
            : base(level) { }

        [EnableDump]
        new int Level => base.Level;

        IParserTokenType<BinaryTree> IBracketMatch<BinaryTree>.Value { get; } = new Matched();

        bool IDefaultScopeProvider.MeansPublic => Level == 3;
        IDefaultScopeProvider ISyntaxScope.DefaultScopeProvider => this;
        bool ISyntaxScope.IsDeclarationPart => false;

        SyntaxFactory.IValueProvider SyntaxFactory.IValueToken.Provider => SyntaxFactory.Bracket;
    }
}