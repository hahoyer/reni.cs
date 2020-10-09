using System.Collections.Generic;
using hw.Parser;
using Reni.Parser;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class Colon : TokenClass, IStatementProvider, SyntaxFactory.IDeclarationsToken
    {
        public const string TokenId = ":";

        public override string Id => TokenId;

        SyntaxFactory.IDeclarationsProvider SyntaxFactory.IDeclarationsToken.Provider => SyntaxFactory.Colon;
    }

    [BelongsTo(typeof(MainTokenFactory))]
    sealed class Exclamation : ParserTokenType<BinaryTree>, PrioParser<BinaryTree>.ISubParserProvider
    {
        public const string TokenId = "!";

        readonly ISubParser<BinaryTree> Parser;

        public Exclamation(ISubParser<BinaryTree> parser) => Parser = parser;

        public override string Id => TokenId;

        ISubParser<BinaryTree> PrioParser<BinaryTree>.ISubParserProvider.NextParser => Parser;

        protected override BinaryTree Create(BinaryTree left, IToken token, BinaryTree right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }
    }


    [BelongsTo(typeof(DeclarationTokenFactory))]
    abstract class DeclarationTagToken
        : TerminalToken, IDeclarerTagProvider, IDeclarationTag, SyntaxFactory.IDeclarerToken
    {
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

        SyntaxFactory.IDeclarerProvider SyntaxFactory.IDeclarerToken.Provider => SyntaxFactory.Declarer;
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