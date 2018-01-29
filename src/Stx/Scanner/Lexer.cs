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

            var signedIntegerTypeName = "SINT".Box(false) | "INT".Box(false) | "DINT".Box(false) | "LINT".Box(false);

            var unsignedIntegerTypeName
                = "USINT".Box(false) |
                  "UINT".Box(false) |
                  "UDINT".Box(false) |
                  "ULINT".Box(false);

            var realTypeName = "REAL".Box(false) | "LREAL".Box(false);

            var integerTypeName = signedIntegerTypeName | unsignedIntegerTypeName;

            var integer = Match.Digit.Else("_").Repeat(1);
            var bit = "01".AnyChar();
            var octalDigit = "01234567".AnyChar();
            var hexDigit = integer | "abcdef".AnyChar(false);

            var signedInteger = "+-".AnyChar().Option() + integer;
            var binaryInteger = "2#" + bit + ("_".Box().Option() + bit).Repeat();
            var octalInteger = "8#" + octalDigit + ("_".Box().Option() + octalDigit).Repeat();
            var hexInteger = "16#" + hexDigit + ("_".Box().Option() + hexDigit).Repeat();
            var integerLiteral = (integerTypeName + "#").Option() +
                                 (signedInteger | binaryInteger | octalInteger | hexInteger);
            var exponent = "E".AnyChar(false) + signedInteger;
            var realLiteral = (realTypeName + "#").Option() + signedInteger + "." + integer + exponent.Option();

            var numericLiteral = integerLiteral | realLiteral;

            Items[new NumericLiteral()] = numericLiteral;

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

            var singleByteCharacterString = "'" + singleByteCharacterRepresentation.Repeat() + "'";
            var doubleByteCharacterString = "'" + doubleByteCharacterRepresentation.Repeat() + "'";

            var characterString = singleByteCharacterString | doubleByteCharacterString;

            Items[new StringLiteral1()] = characterString;

            var fixedPoint = integer + ("." + integer).Option();
            var milliseconds = fixedPoint + "ms".Box(false);
            var seconds = (fixedPoint + "s".Box(false)) |
                          (integer + "s".Box(false) + "_".Box().Option() + milliseconds);
            var minutes = (fixedPoint + "m".Box(false)) | (integer + "m".Box(false) + "_".Box().Option() + seconds);
            var hours = (fixedPoint + "h".Box(false)) | (integer + "h".Box(false) + "_".Box().Option() + minutes);
            var days = (fixedPoint + "d".Box(false)) | (integer + "d".Box(false) + "_".Box().Option() + hours);
            var interval = days | hours | minutes | seconds | milliseconds;
            var duration = ("time".Box(false) | "t".Box(false)) + "#" + "-".Box().Option() + interval;

            var dayHour = integer;
            var dayMinute = integer;
            var daySecond = fixedPoint;
            var daytime = dayHour + ":" + dayMinute + ":" + daySecond;
            var timeOfDay = ("TIME_OF_DAY".Box(false) | "TOD".Box(false)) + "#" + daytime;
            var year = integer;
            var month = integer;
            var day = integer;
            var dateLiteral = year + "-" + month + "-" + day;
            var date = ("date".Box(false) | "d".Box(false)) + "#" + dateLiteral;
            var dateAndTime =  ("DATE_AND_TIME".Box(false) | "DT".Box(false))+ "#"+ dateLiteral +"-"+ daytime;
            var timeLiteral = duration | timeOfDay | date | dateAndTime;
            Items[new Timeiteral()] = timeLiteral;

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