using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Scanner;

namespace hw.Parser
{
    sealed class ReniLexer : ILexer
    {
        sealed class Error : Match.IError
        {
            public readonly string Message;
            public Error(string message) { Message = message; }
            public override string ToString() { return Message; }
        }

        readonly Match _whiteSpaces;
        readonly Match _any;
        readonly Match _text;
        readonly Error _invalidTextEnd = new Error("End of line in string");
        readonly Error _invalidLineComment = new Error("End of line in comment");
        readonly Error _invalidComment = new Error("End of file in comment");
        readonly IMatch _number;

        public ReniLexer()
        {
            var alpha = Match.Letter.Else("_");
            var symbol1 = "({[)}];,".AnyChar();
            var textFrame = "'\"".AnyChar();
            var symbol = "°^!²§³$%&/=?\\@€*+~><|:.-".AnyChar();

            var identifier = (alpha + (alpha.Else(Match.Digit)).Repeat()).Else(symbol.Repeat(1));

            _any = symbol1.Else(identifier);

            _whiteSpaces =
                Match.WhiteSpace.Else("#" + " \t".AnyChar() + Match.LineEnd.Find)
                    .Else("#(" + Match.WhiteSpace + (Match.WhiteSpace + ")#").Find)
                    .Else("#(" + _any.Value(id => (Match.WhiteSpace + id + ")#").Box().Find))
                    .Else("#(" + Match.End.Find + _invalidComment)
                    .Else("#" + Match.End.Find + _invalidLineComment)
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

        int ILexer.WhiteSpace(SourcePosn sourcePosn)
        {
            var result = sourcePosn.Match(_whiteSpaces);
            Tracer.Assert(result != null);
            return result.Value;
        }

        int? ILexer.Number(SourcePosn sourcePosn) { return sourcePosn.Match(_number); }
        int? ILexer.Any(SourcePosn sourcePosn) { return sourcePosn.Match(_any); }
        int? ILexer.Text(SourcePosn sourcePosn) { return sourcePosn.Match(_text); }
    }
}