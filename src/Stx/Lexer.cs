using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using hw.Scanner;

namespace Stx
{
    public sealed class Lexer : Match2TwoLayerScannerGuard
    {
        const string Symbols = "^!%&/=?\\*+~><|:-";
        const string SingleCharSymbol = "({[)}];,.";
        internal static readonly Lexer Instance = new Lexer();

        public static bool IsSymbolLike(string id)
            => id.Length > 0 && Symbols.Contains(id[0]);

        public static bool IsAlphaLike(string id)
        {
            if(id == "")
                return false;

            return char.IsLetter(id[0]) || id[0] == '_';
        }

        readonly Match Any;

        readonly IssueId InvalidTextEnd = IssueId.EOLInString;

        readonly IDictionary<IScannerTokenType, IMatch> Items = new Dictionary<IScannerTokenType, IMatch>();
        readonly IssueId MissingEndOfComment = IssueId.MissingEndOfComment;
        readonly IssueId MissingEndOfPragma = IssueId.MissingEndOfPragma;

        Lexer()
            : base(error => new ScannerSyntaxError((IssueId) error))
        {
            var identifier =
                Match.Letter.Else("_") + Match.Letter.Else("_").Else(Match.Digit).Repeat();

            Any = identifier;

            var sign = "+-".AnyChar();
            var decimalDigits = Match.Digit.Else("_").Repeat(1);
            var integerLiteral = sign.Repeat(maxCount: 1) + decimalDigits;
            var exponent = "E".AnyChar(false) + integerLiteral;

            Items[new WhiteSpaceTokenType("Space")] = " \t".AnyChar();
            Items[new WhiteSpaceTokenType("LineEnd")] = "\r\n".Box().Else("\n".Box()).Else("\r" + Match.End);
            Items[new WhiteSpaceTokenType("MultiLineComment")] =
                "(*" + "*)".Box().Find.Else(Match.End.Find + MissingEndOfComment);
            Items[new WhiteSpaceTokenType("Pragma")] =
                "{" + "}".Box().Find.Else(Match.End.Find + MissingEndOfPragma);

            Items[new IntegerLiteral()] = integerLiteral;
            Items[new RealLiteral()] = integerLiteral + "." + decimalDigits + exponent.Repeat(maxCount: 1);
            Items[new Base2Literal()] = "2#".Box() + "01_".AnyChar().Repeat(1);
            Items[new Base8Literal()] = "8#".Box() + "01234567_".AnyChar().Repeat(1);
            Items[new Base16Literal()] = "16#".Box() + "0123456789abcdef_".AnyChar(false).Repeat(1);
            Items[new StringLiteral1()] = StringLiteral('\'', '$');
            Items[new DurationLiteral()] = "T".Box(false).Else("TIME".Box(false)) + "#" + "0123456789._mshd-+".AnyChar(false).Repeat();
            Items[new DateLTimeiteral()] = Match.Any;
            Items[new DateLiteral()] = Match.Any;
            Items[new Timeiteral()] = Match.Any;
        }

        Match StringLiteral(char delimiter, char escape)
            => (delimiter + "").Box(false) +
               (escape + "" + delimiter).AnyChar(false).Else(Match.LineEnd).Not.Repeat() +
               (escape + "" + Match.LineEnd.Not + (escape + "" + delimiter).AnyChar(false).Else(Match.LineEnd).Not.Repeat())
               .Repeat() +
               (delimiter + "").Box(false).Else(Match.LineEnd + InvalidTextEnd);

        internal LexerItem[] LexerItems(ScannerTokenType<Syntax> scannerTokenType)
            => Items
                .Concat(new[] {new KeyValuePair<IScannerTokenType, IMatch>(scannerTokenType, Any)})
                .Select(i => CreateLexerItem(i.Key, i.Value))
                .ToArray();


        LexerItem CreateLexerItem(IScannerTokenType scannerTokenType, IMatch match)
            => new LexerItem(scannerTokenType, sourcePosn => GuardedMatch(sourcePosn, match));
    }



}