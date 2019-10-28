using Bnf.Forms;
using Bnf.TokenClasses;
using hw.Helper;
using hw.Scanner;

namespace Bnf.Scanner
{
    public sealed class Lexer : hw.Scanner.Lexer
    {
        internal const char StringEscapeChar = '\\';
        internal static readonly Lexer Instance = new Lexer();

        static Match StringLiteral(char delimiter)
        {
            var delimiterString = delimiter.ToString();
            var stringEscapeString = StringEscapeChar.ToString();
            return
                delimiterString +
                (
                    (delimiterString + stringEscapeString + "\t\r\n").AnyChar().Not |
                    (stringEscapeString + (delimiterString + stringEscapeString + "trn").AnyChar())
                )
                .Repeat() +
                delimiterString;
        }

        readonly IssueId InvalidTextEnd = IssueId.EOLInString;

        readonly IssueId MissingEndOfComment = IssueId.MissingEndOfComment;
        readonly IssueId MissingEndOfPragma = IssueId.MissingEndOfPragma;
        readonly FunctionCache<string, ParserLiteral> ParserLiterals;

        Lexer()
            : base(error => new ScannerSyntaxError((IssueId) error))
        {
            ParserLiterals = new FunctionCache<string, ParserLiteral>(name => new ParserLiteral(name));
            Items[new WhiteSpaceTokenType("Space")] = " \t".AnyChar();
            Items[new WhiteSpaceTokenType("LineEnd")] = "\r\n".Box() | "\n".Box() | "\r".Box() | ("\r" + Match.End);

            Items[new StringLiteral('\'', ParserLiterals)] = StringLiteral('\'');
            Items[new StringLiteral('"', ParserLiterals)] = StringLiteral('"');

            var letterPlus = Match.Letter.Else("_");
            var identifier = letterPlus + (letterPlus | Match.Digit).Repeat();

            Any = identifier | "::=".Box() | ";|[]{}()".AnyChar();
        }

        protected override IMatch Any {get;}
    }
}