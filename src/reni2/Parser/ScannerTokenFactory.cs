using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.Parser
{
    sealed class ScannerTokenFactory : DumpableObject, ITokenFactory<Syntax>, Compiler<Syntax>.IComponent
    {
        [EnableDump]
        Compiler<Syntax>.Component Current;

        Compiler<Syntax>.Component Compiler<Syntax>.IComponent.Current
        {
            set => Current = value;
        }

        IParserTokenType<Syntax> ITokenFactory<Syntax>.BeginOfText => new BeginOfText();
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
            new LexerItem(Current.Get<ScannerTokenType<Syntax>>(), Lexer.Instance.Any),
            new LexerItem(new Text(), Lexer.Instance.Text)
        };
    }
}