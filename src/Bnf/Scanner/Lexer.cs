using System.Collections.Generic;
using System.Linq;
using Bnf.TokenClasses;
using hw.Parser;
using hw.Scanner;

namespace Bnf.Scanner
{
    public sealed class Lexer : Match2TwoLayerScannerGuard
    {
        internal static readonly Lexer Instance = new Lexer();

        internal const char StringEscapeChar = '\\';

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

        readonly Match Any;

        readonly IssueId InvalidTextEnd = IssueId.EOLInString;

        readonly IDictionary<IScannerTokenType, IMatch> Items = new Dictionary<IScannerTokenType, IMatch>();
        readonly IssueId MissingEndOfComment = IssueId.MissingEndOfComment;
        readonly IssueId MissingEndOfPragma = IssueId.MissingEndOfPragma;

        Lexer()
            : base(error => new ScannerSyntaxError((IssueId) error))
        {
            Items[new WhiteSpaceTokenType("Space")] = " \t".AnyChar();
            Items[new WhiteSpaceTokenType("LineEnd")] = "\r\n".Box() | "\n".Box() | ("\r" + Match.End);

            Items[new StringLiteral('\'')] = StringLiteral('\'');
            Items[new StringLiteral('"')] = StringLiteral('"');

            var letterPlus = Match.Letter.Else("_");
            var identifier = letterPlus + (letterPlus | Match.Digit).Repeat();

            Any = identifier | "::=".Box() | "<...>".Box() | ";|[]{}()".AnyChar();
        }

        internal LexerItem[] LexerItems(ScannerTokenType<Syntax> scannerTokenType)
            => Items
                .Concat(new[] {new KeyValuePair<IScannerTokenType, IMatch>(scannerTokenType, Any)})
                .Select(i => CreateLexerItem(i.Key, i.Value))
                .ToArray();


        LexerItem CreateLexerItem(IScannerTokenType scannerTokenType, IMatch match)
            => new LexerItem(scannerTokenType, sourcePosn => GuardedMatch(sourcePosn, match));
    }
}