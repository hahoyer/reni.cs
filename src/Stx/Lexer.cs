using System;
using System.Linq;
using hw.Scanner;

namespace Stx
{
    public sealed class Lexer : Match2TwoLayerScannerGuard
    {
        const string Symbols = "^!%&/=?\\*+~><|:-";
        const string SingleCharSymbol = "({[)}];,.";
        internal static readonly Lexer Instance = new Lexer();

        public static bool IsSpace(IItem item)
            => item.ScannerTokenType == Instance.SpaceItem.ScannerTokenType;

        public static bool IsMultiLineComment(IItem item)
            => item.ScannerTokenType == Instance.MultiLineCommentItem.ScannerTokenType;

        public static bool IsLineComment(IItem item)
            => item.ScannerTokenType == Instance.LineCommentItem.ScannerTokenType;

        public static bool IsLineEnd(IItem item)
            => item.ScannerTokenType == Instance.LineEndItem.ScannerTokenType;

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
        readonly IMatch LineComment;
        readonly LexerItem LineCommentItem;
        internal readonly LexerItem LineEndItem;
        readonly IssueId MissingEndOfComment = IssueId.MissingEndOfComment;
        readonly IssueId MissingEndOfPragma = IssueId.MissingEndOfPragma;
        internal readonly LexerItem MultiLineCommentItem;
        internal readonly IMatch Number;
        internal readonly LexerItem PragmaItem;

        internal readonly LexerItem SpaceItem;
        readonly Match Text;
        readonly Match VarbatimTextHead;

        Lexer()
            : base(error => new ScannerSyntaxError((IssueId) error))
        {
            var symbol1 = SingleCharSymbol.AnyChar();
            var textFrame = "'\"".AnyChar();

            var identifier =
                Match.Letter.Else("_") + Match.Letter.Else("_").Else(Match.Digit).Repeat();

            Any = symbol1.Else(identifier);

            var lineEnd = "\r\n".Box().Else("\n".Box()).Else("\r" + Match.End);
            var lineEndOrEnd = lineEnd.Else(Match.End);

            LineComment = "#" +
                          lineEndOrEnd
                              .Else
                              (
                                  "(".AnyChar().Not +
                                  lineEnd.Find
                                      .Else(Match.End.Find)
                              );

            var integerLiteral = Match.Digit.Repeat(1);
            var realLiteral = Match.Digit.Repeat(1);
            var base2Literal = Match.Digit.Repeat(1);
            var base8Literal = Match.Digit.Repeat(1);
            var base16Literal = Match.Digit.Repeat(1);

            Number = integerLiteral
                    .Else(realLiteral)
                    .Else(base2Literal)
                    .Else(base8Literal)
                    .Else(base16Literal)
                ;

            var varbatimText = "@(" +
                               (Match.WhiteSpace + (Match.WhiteSpace + ")@").Find)
                               .Else(identifier.Value(id => (Match.WhiteSpace + id + ")@").Box().Find))
                               .Else(Match.End.Find + InvalidTextEnd)
                ;

            VarbatimTextHead = "@(" + Match.WhiteSpace.Else(identifier);
            Text = textFrame.Value
                (
                    head =>
                    {
                        var textEnd = head.Else(lineEndOrEnd + InvalidTextEnd);
                        return textEnd.Find + (head + textEnd.Find).Repeat();
                    })
                .Else(varbatimText);

            LineCommentItem = new LexerItem(new WhiteSpaceTokenType("LineComment"), MatchLineComment);
            MultiLineCommentItem = CreateLexerItem
            (
                "MultiLineComment",
                "(*" +
                "*)".Box()
                    .Find
                    .Else(Match.End.Find + MissingEndOfComment)
            );
            PragmaItem = CreateLexerItem
            (
                "Pragma",
                "{" +
                "}".Box()
                    .Find
                    .Else(Match.End.Find + MissingEndOfPragma));

            LineEndItem = CreateLexerItem("LineEnd", "\r\n".Box().Else("\n".Box()).Else("\r" + Match.End));
            SpaceItem = CreateLexerItem("Space", " \t".AnyChar());
        }

        LexerItem CreateLexerItem(string whiteSpaceTokenTypeName, IMatch matchExpression)
            => new LexerItem
            (
                new WhiteSpaceTokenType(whiteSpaceTokenTypeName),
                GuardedMatch(matchExpression)
            );

        internal Func<SourcePosn, int?> GuardedMatch(IMatch matchExpression) => sourcePosn => GuardedMatch(sourcePosn, matchExpression);

        internal int? MatchNumber(SourcePosn sourcePosn) => GuardedMatch(sourcePosn, Number);
        internal int? MatchAny(SourcePosn sourcePosn) => GuardedMatch(sourcePosn, Any);
        internal int? MatchText(SourcePosn sourcePosn) => GuardedMatch(sourcePosn, Text);
        int? MatchLineComment(SourcePosn sourcePosn) => GuardedMatch(sourcePosn, LineComment);
    }
}