using Bnf.TokenClasses;
using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;

namespace Bnf.Scanner
{
    sealed class ScannerTokenFactory : DumpableObject, ITokenFactory<Syntax>, Compiler<Syntax>.IComponent
    {
        [EnableDump]
        Compiler<Syntax>.Component Current;

        Compiler<Syntax>.Component Compiler<Syntax>.IComponent.Current {set => Current = value;}

        hw.Scanner.ITokenType ILexerTokenFactory.EndOfText
            => new EndOfText();

        hw.Scanner.ITokenType ILexerTokenFactory.InvalidCharacterError
            => new ScannerSyntaxError(IssueId.InvalidCharacter);

        LexerItem[] ILexerTokenFactory.Classes
            => Lexer.Instance.LexerItems(Current.Get<ITokenTypeFactory>());

        IPriorityParserTokenType<Syntax> ITokenFactory<Syntax>.BeginOfText
            => new BeginOfText();
    }
}