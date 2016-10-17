using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.Parser
{
    abstract class TokenFactory : DumpableObject, ITokenFactory
    {
        internal IEnumerable<IParserTokenType<Syntax>> PredefinedTokenClasses
            => GetType()
                .Assembly
                .GetTypes()
                .Where(BelongsToFactory)
                .SelectMany(CreateInstance);

        IEnumerable<IParserTokenType<Syntax>> CreateInstance(System.Type type)
        {
            var variants = type.GetAttributes<Variant>(false).ToArray();
            if(variants.Any())
                return variants
                    .Select(variant => variant.CreateInstance(type));

            return new[] {SpecialTokenClass(type)};
        }

        protected virtual IParserTokenType<Syntax> SpecialTokenClass(System.Type type)
            => (TokenClass) Activator.CreateInstance(type);

        bool BelongsToFactory(System.Type type)
        {
            var thisType = GetType();
            return type.Is<ScannerTokenType>()
                && !type.IsAbstract
                && type
                    .GetAttributes<BelongsTo>(true)
                    .Any(attr => thisType.Is(attr.TokenFactory));
        }

        IScannerTokenType ITokenFactory.EndOfText => new RightParenthesis(0);

        IScannerTokenType ITokenFactory.InvalidCharacterError
            => new ScannerSyntaxError(IssueId.InvalidCharacter);

        LexerItem[] ITokenFactory.Classes => Classes;

        protected LexerItem[] Classes => new[]
        {
            Lexer.Instance.WhiteSpaceItem,
            Lexer.Instance.LineEndItem,
            Lexer.Instance.CommentItem,
            Lexer.Instance.LineCommentItem,
            new LexerItem(new Number(), Lexer.Instance.Number),
            new LexerItem(new AnyTokenType(this), Lexer.Instance.Any),
            new LexerItem(new Text(), Lexer.Instance.Text)
        };

        internal abstract IParserTokenType<Syntax> GetTokenClass(string name);
    }

    sealed class AnyTokenType : PredefinedTokenFactory<Syntax>
    {
        readonly TokenFactory Parent;
        public AnyTokenType(TokenFactory parent) { Parent = parent; }

        protected override IParserTokenType<Syntax> GetTokenClass(string name)
            => Parent.GetTokenClass(name);

        protected override IEnumerable<IParserTokenType<Syntax>> GetPredefinedTokenClasses()
            => Parent.PredefinedTokenClasses;
    }
}