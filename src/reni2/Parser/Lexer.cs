using System.Linq;
using hw.Scanner;
using Reni.Validation;

namespace Reni.Parser
{
    public sealed class Lexer : Match2TwoLayerScannerGuard
    {
        public class WhiteSpaceToken
        {
            public SourcePart Characters;
        }

        const string Symbols = "^!%&/=?\\*+~><|:-";
        const string SingleCharSymbol = "({[)}];,.";
        internal static readonly Lexer Instance = new Lexer();

        public static bool IsWhiteSpace(IItem item)
            => item.ScannerTokenType == Instance.WhiteSpaceItem.ScannerTokenType;

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
        readonly Match MultiLineComment;
        readonly Match CommentHead;
        readonly IssueId InvalidComment = IssueId.EOFInComment;
        readonly IssueId InvalidLineComment = IssueId.EOFInLineComment;
        readonly IssueId InvalidTextEnd = IssueId.EOLInString;
        readonly IMatch LineComment;
        readonly Match LineEnd;
        readonly Match LineEndOrEnd;
        readonly IMatch Number;
        readonly Match Text;
        readonly Match VarbatimTextHead;
        readonly Match WhiteSpace;
        internal readonly LexerItem LineCommentItem;
        internal readonly LexerItem LineEndItem;
        internal readonly LexerItem MultiLineCommentItem;

        internal readonly LexerItem WhiteSpaceItem;

        Lexer()
            : base(error => new ScannerSyntaxError((IssueId) error))
        {
            var alpha = Match.Letter.Else("_");
            var symbol1 = SingleCharSymbol.AnyChar();
            var textFrame = "'\"".AnyChar();
            var symbol = Symbols.AnyChar();

            var identifier = (alpha + alpha.Else(Match.Digit).Repeat()).Else(symbol.Repeat(1));

            Any = symbol1.Else(identifier);

            LineEnd = "\r\n".Box().Else("\n".Box()).Else("\r" + Match.End);
            LineEndOrEnd = LineEnd.Else(Match.End);

            LineComment = "#" +
                           LineEndOrEnd
                               .Else
                               (
                                   "(".AnyChar().Not +
                                   LineEnd.Find
                                       .Else(Match.End.Find)
                               );

            WhiteSpace = " \t".AnyChar();
            MultiLineComment = "#(" +
                       (Match.WhiteSpace + (Match.WhiteSpace + ")#").Find)
                       .Else(identifier.Value(id => (Match.WhiteSpace + id + ")#").Box().Find))
                       .Else(Match.End.Find + InvalidComment)
                ;

            CommentHead = "#(" + Match.WhiteSpace.Else(identifier);

            Number = Match.Digit.Repeat(1);

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
                        var textEnd = head.Else(LineEndOrEnd + InvalidTextEnd);
                        return textEnd.Find + (head + textEnd.Find).Repeat();
                    })
                .Else(varbatimText);

            LineCommentItem = new LexerItem(new WhiteSpaceTokenType(), MatchLineComment);
            MultiLineCommentItem = new LexerItem(new WhiteSpaceTokenType(), MatchMultiLineComment);
            LineEndItem = new LexerItem(new WhiteSpaceTokenType(), MatchLineEnd);
            WhiteSpaceItem = new LexerItem(new WhiteSpaceTokenType(), MatchWhiteSpace);
        }

        internal int? MatchNumber(SourcePosn sourcePosn) => GuardedMatch(sourcePosn, Number);
        internal int? MatchAny(SourcePosn sourcePosn) => GuardedMatch(sourcePosn, Any);
        internal int? MatchText(SourcePosn sourcePosn) => GuardedMatch(sourcePosn, Text);
        int? MatchWhiteSpace(SourcePosn sourcePosn) => GuardedMatch(sourcePosn, WhiteSpace);
        int? MatchLineEnd(SourcePosn sourcePosn) => GuardedMatch(sourcePosn, LineEnd);
        int? MatchMultiLineComment(SourcePosn sourcePosn) => GuardedMatch(sourcePosn, MultiLineComment);
        int? MatchLineComment(SourcePosn sourcePosn) => GuardedMatch(sourcePosn, LineComment);


        //Match.IError InvalidCharacterError { get; } = new Error(IssueId.InvalidCharacter);

        public string WhiteSpaceId(IItem item)
        {
            if(!IsMultiLineComment(item))
                return null;

            var headEnd = item.SourcePart.Start.Match(CommentHead);
            if(headEnd == null)
                return null;

            return item.SourcePart.Start.Span(headEnd.Value).Id;
        }

        internal string ExtractText(SourcePart token)
        {
            var headLength = token.Start.Match(VarbatimTextHead);
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
    }
}