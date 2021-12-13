using System.Linq;
using hw.Scanner;
using Reni.Validation;

namespace Reni.Parser
{
    public sealed class Lexer : Match2TwoLayerScannerGuard
    {
        const string Symbols = "^!%&/=?\\*@+~><|:-";
        const string SingleCharSymbol = "({[)}];,.";
        internal static readonly Lexer Instance = new();
        internal readonly Match LineEnd;
        internal readonly LexerItem InlineCommentItem;
        internal readonly LexerItem WhiteSpacesItem;
        internal readonly IMatch LineComment;
        internal readonly IMatch Space;
        internal readonly IMatch InlineComment;
        internal readonly Match LineCommentHead;
        internal readonly Match InlineCommentHead;
        internal readonly Match InlineCommentTail;
        internal readonly Match LineEndOrEnd;

        readonly IMatch Any;
        readonly IssueId InvalidComment = IssueId.EOFInComment;
        readonly IssueId InvalidTextEnd = IssueId.EOLInString;
        readonly IMatch Number;
        readonly IMatch Text;
        readonly IMatch VerbatimTextHead;
        readonly LexerItem LineCommentItem;
        readonly LexerItem LineEndItem;
        readonly LexerItem SpaceItem;
        readonly IMatch WhiteSpaces;

        Lexer()
            : base(error => new ScannerSyntaxError((IssueId)error))
        {
            var alpha = Match.Letter.Else("_");
            var symbol1 = SingleCharSymbol.AnyChar();
            var textFrame = "'\"".AnyChar();
            var symbol = Symbols.AnyChar();

            var identifier = (alpha + alpha.Else(Match.Digit).Repeat()).Else(symbol.Repeat(1));

            Any = symbol1.Else(identifier);

            LineEnd = "\r\n".Box().Else("\n".Box()).Else("\r" + Match.End);
            LineEndOrEnd = LineEnd.Else(Match.End);

            LineCommentHead = "#".Box();
            LineComment = LineCommentHead +
                LineEndOrEnd
                    .Else
                    (
                        "(".AnyChar().Not +
                        LineEnd.Find
                            .Else(Match.End.Find)
                    );

            Space = " \t".AnyChar();
            InlineCommentHead = "#(".Box();
            InlineCommentTail = ")#".Box();

            InlineComment = InlineCommentHead +
                InlineCommentTail
                    .Else(Match.WhiteSpace + InlineCommentTail.Else((Match.WhiteSpace + InlineCommentTail).Find))
                    .Else(identifier.Value(id => (Match.WhiteSpace + id + InlineCommentTail).Box().Find))
                    .Else(Match.End.Find + InvalidComment)
                ;

            Number = Match.Digit.Repeat(1);

            var verbatimText = "@(" +
                    (Match.WhiteSpace + (Match.WhiteSpace + ")@").Find)
                    .Else(identifier.Value(id => (Match.WhiteSpace + id + ")@").Box().Find))
                    .Else(Match.End.Find + InvalidTextEnd)
                ;

            VerbatimTextHead = "@(" + Match.WhiteSpace.Else(identifier);
            Text = textFrame.Value
                (
                    head =>
                    {
                        var textEnd = head.Else(LineEndOrEnd + InvalidTextEnd);
                        return textEnd.Find + (head + textEnd.Find).Repeat();
                    })
                .Else(verbatimText);
            WhiteSpaces = LineComment.Else(LineEnd).Else(Space).Repeat(1);

            LineCommentItem = new(new WhiteSpaceTokenType("LineComment"), MatchLineComment);
            InlineCommentItem = new(new WhiteSpaceTokenType("InlineComment"), MatchInlineComment);
            LineEndItem = new(new WhiteSpaceTokenType("LineEnd"), MatchLineEnd);
            SpaceItem = new(new WhiteSpaceTokenType("Space"), MatchSpace);
            WhiteSpacesItem = new(new WhiteSpaceTokenType("WhiteSpaces"), MatchWhiteSpaces);
        }

        public static bool IsSpace(IItem item)
            => item.ScannerTokenType == Instance.SpaceItem.ScannerTokenType;

        public static bool IsMultiLineComment(IItem item)
            => item.ScannerTokenType == Instance.InlineCommentItem.ScannerTokenType;

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

        internal int? MatchNumber(SourcePosition sourcePosition) => GuardedMatch(sourcePosition, Number);
        internal int? MatchAny(SourcePosition sourcePosition) => GuardedMatch(sourcePosition, Any);
        internal int? MatchText(SourcePosition sourcePosition) => GuardedMatch(sourcePosition, Text);
        int? MatchSpace(SourcePosition sourcePosition) => GuardedMatch(sourcePosition, Space);
        int? MatchLineEnd(SourcePosition sourcePosition) => GuardedMatch(sourcePosition, LineEnd);
        int? MatchInlineComment(SourcePosition sourcePosition) => GuardedMatch(sourcePosition, InlineComment);
        int? MatchLineComment(SourcePosition sourcePosition) => GuardedMatch(sourcePosition, LineComment);
        int? MatchWhiteSpaces(SourcePosition sourcePosition) => GuardedMatch(sourcePosition, WhiteSpaces);

        internal string ExtractText(SourcePart token)
        {
            var headLength = token.Start.Match(VerbatimTextHead);
            if(headLength != null)
                return (token.Start + headLength.Value).Span(token.End + -headLength.Value).Id;

            var result = "";
            for(var i = 1; i < token.Length - 1; i++)
            {
                result += (token.Start + i).Current;
                if((token.Start + i).Current == token.Start.Current)
                    i++;
            }

            return result;
        }

        public bool HasComment(SourcePart sourcePart)
            => sourcePart
                .Match(LineComment.Else(InlineComment).Find).HasValue;

        public static bool IsMultiLineCommentEnd(IItem item)
        {
            NotImplementedFunction(item);
            return default;
        }
    }
}