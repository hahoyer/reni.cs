using System.Linq;
using System.Collections.Generic;
using System;
using hw.Parser;
using hw.Scanner;
using Reni.Validation;

namespace Reni.Parser
{
    sealed class ReniLexer : ILexer
    {
        internal static readonly ReniLexer Instance = new ReniLexer();

        sealed class Error : Match.IError
        {
            public readonly IssueId IssueId;
            public Error(IssueId issueId) { IssueId = issueId; }
            public override string ToString() => IssueId.Tag;
        }

        readonly Match _any;
        readonly Match _text;
        readonly Error _invalidTextEnd = new Error(IssueId.EOLInString);
        readonly Error _invalidLineComment = new Error(IssueId.EOFInLineComment);
        readonly Error _invalidComment = new Error(IssueId.EOFInComment);
        readonly IMatch _number;
        readonly Match _lineComment;
        readonly Match _comment;
        readonly Match _whiteSpace;
        readonly Match _commentHead;
        readonly Match _lineEnd;

        ReniLexer()
        {
            var alpha = Match.Letter.Else("_");
            var symbol1 = "({[)}];,".AnyChar();
            var textFrame = "'\"".AnyChar();
            var symbol = "°^!²§³$%&/=?\\@€*+~><|:.-".AnyChar();

            var identifier = (alpha + (alpha.Else(Match.Digit)).Repeat()).Else(symbol.Repeat(1));

            _any = symbol1.Else(identifier);

            _lineComment = "#"
                +
                Match.LineEnd
                    .Else(Match.End)
                    .Else
                    (
                        "(".AnyChar().Not + Match.LineEnd.Find
                            .Else(Match.End.Find + _invalidLineComment)
                    );

            _whiteSpace = " \t\r".AnyChar().Repeat(1);

            _lineEnd = "\n".Box();

            _comment = "#("
                +
                (Match.WhiteSpace + (Match.WhiteSpace + ")#").Find)
                    .Else(_any.Value(id => (Match.WhiteSpace + id + ")#").Box().Find))
                    .Else(Match.End.Find + _invalidComment)
                ;

            _commentHead = "#(" + Match.WhiteSpace.Else(_any);

            _number = Match.Digit.Repeat(1);

            _text = textFrame.Value
                (
                    head =>
                    {
                        var textEnd = head.Else(Match.LineEnd + _invalidTextEnd);
                        return textEnd.Find + (head + textEnd.Find).Repeat();
                    });
        }


        public static bool IsWhiteSpace(WhiteSpaceToken item) => item.Index == 0;
        public static bool IsComment(WhiteSpaceToken item) => item.Index == 1;
        public static bool IsLineComment(WhiteSpaceToken item) => item.Index == 2;
        public static bool IsLineEnd(WhiteSpaceToken item) => item.Index == 3;

        Func<SourcePosn, int?>[] ILexer.WhiteSpace
            => new Func<SourcePosn, int?>[]
            {
                WhiteSpace,
                Comment,
                LineComment,
                LineEnd
            };


        int? ILexer.Number(SourcePosn sourcePosn) => sourcePosn.Match(_number);
        int? ILexer.Any(SourcePosn sourcePosn) => sourcePosn.Match(_any);
        int? ILexer.Text(SourcePosn sourcePosn) => sourcePosn.Match(_text);
        public static IssueId Parse(Match.IError error) => ((Error) error).IssueId;

        int? WhiteSpace(SourcePosn sourcePosn) => sourcePosn.Match(_whiteSpace);
        int? LineEnd(SourcePosn sourcePosn) => sourcePosn.Match(_lineEnd);
        int? Comment(SourcePosn sourcePosn) => sourcePosn.Match(_comment);
        int? LineComment(SourcePosn sourcePosn) => sourcePosn.Match(_lineComment);

        public string WhiteSpaceId(WhiteSpaceToken item)
        {
            if(!IsComment(item))
                return null;

            var headEnd = item.Characters.Start.Match(_commentHead);
            if(headEnd == null)
                return null;

            return item.Characters.Start.Span(headEnd.Value).Id;
        }
    }
}