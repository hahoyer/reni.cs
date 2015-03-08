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
        internal static readonly ILexer Instance = new ReniLexer();

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

        ReniLexer()
        {
            var alpha = Match.Letter.Else("_");
            var symbol1 = "({[)}];,".AnyChar();
            var textFrame = "'\"".AnyChar();
            var symbol = "�^!���$%&/=?\\@�*+~><|:.-".AnyChar();

            var identifier = (alpha + (alpha.Else(Match.Digit)).Repeat()).Else(symbol.Repeat(1));

            _any = symbol1.Else(identifier);

            _lineComment = "#"
                +
                Match.LineEnd
                    .Else(Match.End)
                    .Else
                    (
                        " \t".AnyChar() + Match.LineEnd.Find
                            .Else(Match.End.Find + _invalidLineComment)
                    );

            _whiteSpace = Match.WhiteSpace.Repeat(1);

            _comment = "#("
                +
                (Match.WhiteSpace + (Match.WhiteSpace + ")#").Find)
                    .Else(_any.Value(id => (Match.WhiteSpace + id + ")#").Box().Find))
                    .Else(Match.End.Find + _invalidComment)
                ;

            _number = Match.Digit.Repeat(1);

            _text = textFrame.Value
                (
                    head =>
                    {
                        var textEnd = head.Else(Match.LineEnd + _invalidTextEnd);
                        return textEnd.Find + (head + textEnd.Find).Repeat();
                    });
        }

        Func<SourcePosn, int?>[] ILexer.WhiteSpace
            => new Func<SourcePosn, int?>[]
            {
                WhiteSpace,
                Comment,
                LineComment
            };
        int? ILexer.Number(SourcePosn sourcePosn) => sourcePosn.Match(_number);
        int? ILexer.Any(SourcePosn sourcePosn) => sourcePosn.Match(_any);
        int? ILexer.Text(SourcePosn sourcePosn) => sourcePosn.Match(_text);
        public static IssueId Parse(Match.IError error) => ((Error) error).IssueId;

        int? WhiteSpace(SourcePosn sourcePosn) => sourcePosn.Match(_whiteSpace);
        int? Comment(SourcePosn sourcePosn) => sourcePosn.Match(_comment);
        int? LineComment(SourcePosn sourcePosn) => sourcePosn.Match(_lineComment);
    }
}