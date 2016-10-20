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
        protected TokenFactory()
        {
            PredefinedTokenClasses = GetType()
                .Assembly
                .GetTypes()
                .Where(BelongsToFactory)
                .SelectMany(CreateInstance);
        }

        internal readonly IEnumerable<IParserTokenType<Syntax>> PredefinedTokenClasses;

        IEnumerable<IParserTokenType<Syntax>> CreateInstance(System.Type type)
        {
            var variants = type.GetAttributes<Variant>(true).ToArray();
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

        IScannerTokenType ITokenFactory.EndOfText => new EndOfText();

        IScannerTokenType ITokenFactory.InvalidCharacterError
            => new ScannerSyntaxError(IssueId.InvalidCharacter);

        LexerItem[] ITokenFactory.Classes => Classes;

        LexerItem[] Classes => new[]
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
}