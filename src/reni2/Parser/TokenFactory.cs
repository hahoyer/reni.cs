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
        internal IEnumerable<ScannerTokenType> TokenClasses
            => GetType()
                .Assembly
                .GetTypes()
                .Where(BelongsToFactory)
                .SelectMany(CreateInstance);

        IEnumerable<ScannerTokenType> CreateInstance(System.Type type)
        {
            var variants = type.GetAttributes<Variant>(false).ToArray();
            if(variants.Any())
                return variants
                    .Select(variant => variant.CreateInstance(type));

            return new[] {SpecialTokenClass(type)};
        }

        protected virtual ScannerTokenType SpecialTokenClass(System.Type type)
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
            new LexerItem(NumberTokenType, Lexer.Instance.Number),
            new LexerItem(AnyTokenType, Lexer.Instance.Any),
            new LexerItem(TextTokenType, Lexer.Instance.Text)
        };

        protected abstract IScannerTokenType AnyTokenType { get; }
        protected abstract IScannerTokenType TextTokenType { get; }
        protected abstract IScannerTokenType NumberTokenType { get; }
    }
}