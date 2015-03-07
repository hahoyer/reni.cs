using System.Linq;
using System.Collections.Generic;
using System;
using hw.Debug;
using hw.Parser;
using hw.Scanner;
using Reni.Validation;

namespace Reni.Parser
{
    interface ILexerForUserInterface
    {
        int? PlainWhiteSpace(SourcePosn sourcePosn);
        int? Comment(SourcePosn sourcePosn);
        int? LineComment(SourcePosn sourcePosn);
    }

    sealed class ReniLexer : ILexer, ILexerForUserInterface
    {
        internal static readonly ILexer Instance = new ReniLexer();
        internal static readonly ILexerForUserInterface LexerForUserInterfaceInstance =
            (ILexerForUserInterface) Instance;

        sealed class Error : Match.IError
        {
            public readonly IssueId IssueId;
            public Error(IssueId issueId) { IssueId = issueId; }
            public override string ToString() => IssueId.Tag;
        }

        readonly Match _whiteSpaces;
        readonly Match _any;
        readonly Match _text;
        readonly Error _invalidTextEnd = new Error(IssueId.EOLInString);
        readonly Error _invalidLineComment = new Error(IssueId.EOFInLineComment);
        readonly Error _invalidComment = new Error(IssueId.EOFInComment);
        readonly IMatch _number;
        readonly Match _lineComment;
        readonly Match _comment;

        ReniLexer()
        {
            var alpha = Match.Letter.Else("_");
            var symbol1 = "({[)}];,".AnyChar();
            var textFrame = "'\"".AnyChar();
            var symbol = "°^!²§³$%&/=?\\@€*+~><|:.-".AnyChar();

            var identifier = (alpha + (alpha.Else(Match.Digit)).Repeat()).Else(symbol.Repeat(1));

            _any = symbol1.Else(identifier);

            _lineComment = "#" + " \t".AnyChar() + Match.LineEnd.Find;

            _comment = ("#(" + Match.WhiteSpace + (Match.WhiteSpace + ")#").Find)
                .Else("#(" + _any.Value(id => (Match.WhiteSpace + id + ")#").Box().Find))
                .Else("#(" + Match.End.Find + _invalidComment)
                .Else("#" + Match.End.Find + _invalidLineComment);

            _whiteSpaces =
                Match.WhiteSpace
                    .Else(_lineComment)
                    .Else(_comment)
                    .Repeat();

            _number = Match.Digit.Repeat(1);

            _text = textFrame.Value
                (
                    head =>
                    {
                        var textEnd = head.Else(Match.LineEnd + _invalidTextEnd);
                        return textEnd.Find + (head + textEnd.Find).Repeat();
                    });
        }

        int ILexer.WhiteSpace(SourcePosn sourcePosn) => WhiteSpace(sourcePosn);

        int WhiteSpace(SourcePosn sourcePosn)
        {
            var result = sourcePosn.Match(_whiteSpaces);
            Tracer.Assert(result != null);
            return result.Value;
        }

        int? ILexer.Number(SourcePosn sourcePosn) => sourcePosn.Match(_number);
        int? ILexer.Any(SourcePosn sourcePosn) => sourcePosn.Match(_any);
        int? ILexer.Text(SourcePosn sourcePosn) => sourcePosn.Match(_text);
        public static IssueId Parse(Match.IError error) => ((Error) error).IssueId;

        int? ILexerForUserInterface.PlainWhiteSpace(SourcePosn sourcePosn)
            => sourcePosn.Match(Match.WhiteSpace.Repeat(1));
        int? ILexerForUserInterface.Comment(SourcePosn sourcePosn) => sourcePosn.Match(_lineComment);
        int? ILexerForUserInterface.LineComment(SourcePosn sourcePosn) => sourcePosn.Match(_comment);
    }
}