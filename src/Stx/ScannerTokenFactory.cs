using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;

namespace Stx
{
    sealed class ScannerTokenFactory : DumpableObject, ITokenFactory<Syntax>, Compiler<Syntax>.IComponent
    {
        [EnableDump]
        Compiler<Syntax>.Component Current;

        Compiler<Syntax>.Component Compiler<Syntax>.IComponent.Current {set => Current = value;}

        IParserTokenType<Syntax> ITokenFactory<Syntax>.BeginOfText => new BeginOfText();
        IScannerTokenType ITokenFactory.EndOfText => new EndOfText();

        IScannerTokenType ITokenFactory.InvalidCharacterError
            => new ScannerSyntaxError(IssueId.InvalidCharacter);

        LexerItem[] ITokenFactory.Classes => Classes;

        LexerItem[] Classes => Lexer.Instance.LexerItems(Current.Get<ScannerTokenType<Syntax>>());
    }
}