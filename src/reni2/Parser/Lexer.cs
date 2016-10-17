using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using hw.Scanner;
using Reni.Validation;

namespace Reni.Parser
{
    public sealed class Lexer
    {
        const string Symbols = "^!%&/=?\\*+~><|:-";
        const string SingleCharSymbol = "({[)}];,.";
        internal static readonly Lexer Instance = new Lexer();

        internal readonly LexerItem WhiteSpaceItem;
        internal readonly LexerItem LineEndItem;
        internal readonly LexerItem CommentItem;
        internal readonly LexerItem LineCommentItem;

        readonly Match _any;
        readonly Match _text;
        readonly IssueId _invalidTextEnd = IssueId.EOLInString;
        readonly IssueId _invalidLineComment = IssueId.EOFInLineComment;
        readonly IssueId _invalidComment = IssueId.EOFInComment;
        readonly IMatch _number;
        readonly Match _lineComment;
        readonly Match _comment;
        readonly Match _whiteSpace;
        readonly Match _commentHead;
        readonly Match _lineEnd;
        readonly Match _varbatimTextHead;
        readonly Match _lineEndOrEnd;

        Lexer()
        {
            var alpha = Match.Letter.Else("_");
            var symbol1 = SingleCharSymbol.AnyChar();
            var textFrame = "'\"".AnyChar();
            var symbol = Symbols.AnyChar();

            var identifier = (alpha + alpha.Else(Match.Digit).Repeat()).Else(symbol.Repeat(1));

            _any = symbol1.Else(identifier);

            _lineEnd = "\r\n".Box().Else("\n".Box()).Else("\r" + Match.End);
            _lineEndOrEnd = _lineEnd.Else(Match.End);

            _lineComment = "#"
                +
                _lineEndOrEnd
                    .Else
                    (
                        "(".AnyChar().Not +
                        _lineEnd.Find
                            .Else(Match.End.Find)
                    );

            _whiteSpace = " \t".AnyChar();
            _comment = "#("
                +
                (Match.WhiteSpace + (Match.WhiteSpace + ")#").Find)
                    .Else(identifier.Value(id => (Match.WhiteSpace + id + ")#").Box().Find))
                    .Else(Match.End.Find + _invalidComment)
                ;

            _commentHead = "#(" + Match.WhiteSpace.Else(identifier);

            _number = Match.Digit.Repeat(1);

            var varbatimText = "@("
                    +
                    (Match.WhiteSpace + (Match.WhiteSpace + ")@").Find)
                        .Else(identifier.Value(id => (Match.WhiteSpace + id + ")@").Box().Find))
                        .Else(Match.End.Find + _invalidTextEnd)
                ;

            _varbatimTextHead = "@(" + Match.WhiteSpace.Else(identifier);
            _text = textFrame.Value
                (
                    head =>
                    {
                        var textEnd = head.Else(_lineEndOrEnd + _invalidTextEnd);
                        return textEnd.Find + (head + textEnd.Find).Repeat();
                    })
                .Else(varbatimText);

            LineCommentItem = new LexerItem(new WhiteSpaceTokenType(), LineComment);
            CommentItem = new LexerItem(new WhiteSpaceTokenType(), Comment);
            LineEndItem = new LexerItem(new WhiteSpaceTokenType(), LineEnd);
            WhiteSpaceItem = new LexerItem(new WhiteSpaceTokenType(), WhiteSpace);
        }

        public static bool IsWhiteSpace(IItem item)
            => item.ScannerTokenType == Instance.WhiteSpaceItem.ScannerTokenType;

        public static bool IsComment(IItem item)
            => item.ScannerTokenType == Instance.CommentItem.ScannerTokenType;

        public static bool IsLineComment(IItem item)
            => item.ScannerTokenType == Instance.LineCommentItem.ScannerTokenType;

        public static bool IsLineEnd(IItem item)
            => item.ScannerTokenType == Instance.LineEndItem.ScannerTokenType;

        internal int? Number(SourcePosn sourcePosn) => sourcePosn.Match(_number);
        internal int? Any(SourcePosn sourcePosn) => sourcePosn.Match(_any);
        internal int? Text(SourcePosn sourcePosn) => sourcePosn.Match(_text);

        int? WhiteSpace(SourcePosn sourcePosn) => sourcePosn.Match(_whiteSpace);
        int? LineEnd(SourcePosn sourcePosn) => sourcePosn.Match(_lineEnd);
        int? Comment(SourcePosn sourcePosn) => sourcePosn.Match(_comment);
        int? LineComment(SourcePosn sourcePosn) => sourcePosn.Match(_lineComment);


        //Match.IError InvalidCharacterError { get; } = new Error(IssueId.InvalidCharacter);

        public string WhiteSpaceId(IItem item)
        {
            if(!IsComment(item))
                return null;

            var headEnd = item.SourcePart.Start.Match(_commentHead);
            if(headEnd == null)
                return null;

            return item.SourcePart.Start.Span(headEnd.Value).Id;
        }

        public static bool IsSymbolLike(string id)
            => id.Length > 0 && Symbols.Contains(id[0]);

        public static bool IsAlphaLike(string id)
        {
            if(id == "")
                return false;

            return char.IsLetter(id[0]) || id[0] == '_';
        }

        internal string ExtractText(SourcePart token)
        {
            var headLength = token.Start.Match(_varbatimTextHead);
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

        public class WhiteSpaceToken
        {
            public SourcePart Characters;
        }
    }
}