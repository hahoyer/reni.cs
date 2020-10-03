using System.Collections.Generic;
using hw.Parser;
using Reni.Parser;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class Colon : TokenClass, IStatementProvider
    {
        public const string TokenId = ":";
        public override string Id => TokenId;

        Result<Statement> IStatementProvider.Get(BinaryTree left, BinaryTree right, ISyntaxScope scope)
        {
            return left.Declarer?.Convert(x => x.Statement(right.Syntax(scope), scope.DefaultScopeProvider));
        }
    }

    [BelongsTo(typeof(MainTokenFactory))]
    sealed class Exclamation : ParserTokenType<BinaryTree>,
        PrioParser<BinaryTree>.ISubParserProvider
    {
        public const string TokenId = "!";

        readonly ISubParser<BinaryTree> Parser;

        public Exclamation(ISubParser<BinaryTree> parser) { Parser = parser; }

        public override string Id => TokenId;

        protected override BinaryTree Create(BinaryTree left, IToken token, BinaryTree right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }

        ISubParser<BinaryTree> PrioParser<BinaryTree>.ISubParserProvider.NextParser => Parser;
    }


    [BelongsTo(typeof(DeclarationTokenFactory))]
    abstract class DeclarationTagToken : 
        TerminalToken, 
        IDeclarerTagProvider, 
        IDeclarationTag
    {
        Result<Declarer> IDeclarerTagProvider.Get
            (BinaryTree binaryTree)
        {
            if(binaryTree.Left == null && binaryTree.Right == null)
                return new Declarer(new IDeclarationTag[] {this}, null,T(binaryTree));

            NotImplementedMethod(binaryTree);
            return null;
        }

        public static IEnumerable<string> DeclarationOptions
        {
            get
            {
                yield return ConverterToken.TokenId;
                yield return MutableDeclarationToken.TokenId;
                yield return MixInDeclarationToken.TokenId;
                yield return NonPositionalDeclarationToken.TokenId;
                yield return NonPublicDeclarationToken.TokenId;
                yield return PositionalDeclarationToken.TokenId;
                yield return PublicDeclarationToken.TokenId;
            }
        }
    }

    sealed class ConverterToken : DeclarationTagToken
    {
        internal const string TokenId = "converter";
        public override string Id => TokenId;
    }

    sealed class MutableDeclarationToken : DeclarationTagToken
    {
        internal const string TokenId = "mutable";
        public override string Id => TokenId;
    }

    sealed class MixInDeclarationToken : DeclarationTagToken
    {
        internal const string TokenId = "mix_in";
        public override string Id => TokenId;
    }

    sealed class PublicDeclarationToken : DeclarationTagToken
    {
        internal const string TokenId = "public";
        public override string Id => TokenId;
    }

    sealed class NonPublicDeclarationToken : DeclarationTagToken
    {
        internal const string TokenId = "non_public";
        public override string Id => TokenId;
    }

    sealed class PositionalDeclarationToken : DeclarationTagToken
    {
        internal const string TokenId = "positional";
        public override string Id => TokenId;
    }

    sealed class NonPositionalDeclarationToken : DeclarationTagToken
    {
        internal const string TokenId = "non_positional";
        public override string Id => TokenId;
    }
}