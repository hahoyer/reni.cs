using Bnf.TokenClasses;
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
                    (delimiterString + stringEscapeString + "\r\n").AnyChar().Not |
                    (stringEscapeString + (delimiterString + stringEscapeString + "rn").AnyChar())
                )
                .Repeat() +
                delimiterString;
        }

        readonly IssueId InvalidTextEnd = IssueId.EOLInString;

        readonly IssueId MissingEndOfComment = IssueId.MissingEndOfComment;
        readonly IssueId MissingEndOfPragma = IssueId.MissingEndOfPragma;

        Lexer()
            : base(error => new ScannerSyntaxError((IssueId) error))
        {
            Items[new WhiteSpaceTokenType("Space")] = " \t".AnyChar();
            Items[new WhiteSpaceTokenType("LineEnd")] = "\r\n".Box() | "\n".Box() | "\r".Box() | ("\r" + Match.End);

            Items[new StringLiteral('\'')] = StringLiteral('\'');
            Items[new StringLiteral('"')] = StringLiteral('"');

            var letterPlus = Match.Letter.Else("_");
            var identifier = letterPlus + (letterPlus | Match.Digit).Repeat();

            Any = identifier | "::=".Box() | "<...>".Box() | ";|[]{}()".AnyChar();
        }

        protected override IMatch Any {get;}
    }
}