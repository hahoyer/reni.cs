using System;
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
            , ISyntaxFactoryToken
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

        [DisableDump]
        internal override bool IsVisible => false;

        bool IBelongingsMatcher.IsBelongingTo(IBelongingsMatcher otherMatcher)
            => otherMatcher is BeginOfText;

        IParserTokenType<BinaryTree> IBracketMatch<BinaryTree>.Value { get; } = new Matched();
        bool IDefaultScopeProvider.MeansPublic => true;

        [Obsolete("", true)]
        Result<Statement[]> IStatementsProvider.Get(List type, BinaryTree right, ISyntaxScope scope)
        {
            Tracer.Assert(type == null);
            Tracer.Assert(right != null);

            Tracer.Assert(right.Left != null);
            Tracer.Assert(right.TokenClass is EndOfText);
            Tracer.Assert(right.Right == null);

            Tracer.Assert(right.Left.Left == null);
            Tracer.Assert(right.Left.TokenClass is BeginOfText);

            Tracer.Assert(scope == null);

            var result = right.Left.Right;
            return result?.GetStatements(this) ?? new Result<Statement[]>(new Statement[0]);
        }

        ISyntaxFactory ISyntaxFactoryToken.Provider => SyntaxFactory.Bracket;

        IDefaultScopeProvider ISyntaxScope.DefaultScopeProvider => this;

        bool ISyntaxScope.IsDeclarationPart => false;
    }

    sealed class BeginOfText : TokenClass, IBelongingsMatcher
    {
        const string TokenId = PrioTable.BeginOfText;

        [DisableDump]
        public override string Id => TokenId;

        bool IBelongingsMatcher.IsBelongingTo(IBelongingsMatcher otherMatcher)
            => otherMatcher is EndOfText;
    }
}