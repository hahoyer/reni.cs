using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using hw.Scanner;
using Stx.TokenClasses;

namespace Stx.Scanner
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
            Items[new WhiteSpaceTokenType("Space")] = " \t".AnyChar();
            Items[new WhiteSpaceTokenType("LineEnd")] = "\r\n".Box().Else("\n".Box()).Else("\r" + Match.End);
            Items[new WhiteSpaceTokenType("MultiLineComment")] =
                "(*" + "*)".Box().Find.Else(Match.End.Find + MissingEndOfComment);
            Items[new WhiteSpaceTokenType("Pragma")] =
                "{" + "}".Box().Find.Else(Match.End.Find + MissingEndOfPragma);

            var integer = Match.Digit.Else("_").Repeat(1);
            var bit = "01".AnyChar();
            var octalDigit = "01234567".AnyChar();
            var hexDigit = integer | "abcdef".AnyChar(false);
            var signedInteger = "+-".AnyChar().Option() + integer;

            Items[new IntegerLiteral()] = signedInteger;
            Items[new BinaryLiteral()] = "2#" + bit + ("_".Box().Option() + bit).Repeat();
            Items[new OctalLiteral()] = "8#" + octalDigit + ("_".Box().Option() + octalDigit).Repeat();
            Items[new HexLiteral()] = "16#" + hexDigit + ("_".Box().Option() + hexDigit).Repeat();
            Items[new RealLiteral()] = signedInteger + "." + integer + ("E".AnyChar(false) + signedInteger).Option();

            var commonCharacterRepresentation
                = "$\"'".AnyChar().Not |
                  "$$".Box() |
                  "$L".Box() |
                  "$N".Box() |
                  "$P".Box() |
                  "$R".Box() |
                  "$T".Box() |
                  "$l".Box() |
                  "$n".Box() |
                  "$p".Box() |
                  "$r".Box() |
                  "$t".Box();
            ;
            var singleByteCharacterRepresentation =
                commonCharacterRepresentation | "$'".Box() | "\"".Box() | ("$" + hexDigit + hexDigit);
            var doubleByteCharacterRepresentation =
                commonCharacterRepresentation | "$\"".Box() | "'".Box() | ("$" + hexDigit + hexDigit);

            Items[new StringLiteral1()] = "'" + singleByteCharacterRepresentation.Repeat() + "'";
            Items[new StringLiteral2()] = "'" + doubleByteCharacterRepresentation.Repeat() + "'";

            Items[new DurationLiteral()]
                = ("time".Box(false) | "t".Box(false)) +
                  "#" +
                  "-".Box().Option() +
                  (Match.Digit + (Match.Digit | "_.dhms".AnyChar(false)).Repeat());

            var daytime = integer + ":" + integer + ":" + (integer + ("." + integer).Option());
            var dateLiteral = integer + "-" + integer + "-" + integer;

            Items[new TimeLiteral()] = ("TIME_OF_DAY".Box(false) | "TOD".Box(false)) + "#" + daytime ;
            Items[new DateLiteral()] = ("date".Box(false) | "d".Box(false)) + "#" + dateLiteral;
            Items[new DateTimeLiteral()] = ("DATE_AND_TIME".Box(false) | "DT".Box(false)) + "#" + dateLiteral + "-" + daytime;

            var identifier =
                Match.Letter.Else("_") + Match.Letter.Else("_").Else(Match.Digit).Repeat();

            Any = identifier | ":=".Box() | ";:[]()".AnyChar();
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