using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using ReniUI.Classification;

namespace ReniVSIX
{
    static class Extension
    {
        internal static int GetCaretPosition(this IVsTextView textView)
        {
            var rc = textView.GetCaretPos(out var piLine, out var piColumn);
            (rc == VSConstants.S_OK).Assert();
            rc = textView.GetNearestPosition(piLine, piColumn, out var position, out var _);
            (rc == VSConstants.S_OK).Assert();
            return position;
        }

        internal static int GetLineCount(this IVsTextLines vsTextLines)
        {
            vsTextLines.GetLineCount(out var result);
            return result;
        }

        internal static int LinePosition(this IVsTextLines vsTextLines, int lineIndex)
        {
            vsTextLines.GetPositionOfLine(lineIndex, out var result);
            return result;
        }

        internal static int LineIndex(this IVsTextLines vsTextLines, int position)
        {
            vsTextLines.GetLineIndexOfPosition(position, out var result, out var _);
            return result;
        }

        internal static int LineLength(this IVsTextLines vsTextLines, int lineIndex)
        {
            vsTextLines.GetLengthOfLine(lineIndex, out var result);
            return result;
        }

        internal static string GetAll(this IVsTextLines vsTextLines)
        {
            vsTextLines.GetLineCount(out var lineCount);
            vsTextLines.GetLengthOfLine(lineCount - 1, out var lengthOfLastLine);
            vsTextLines.GetLineText(0, 0, lineCount - 1, lengthOfLastLine, out var result);
            return result;
        }

        internal static string Line(IVsTextLines vsTextLines, int index)
        {
            vsTextLines.GetLengthOfLine(index, out var length);
            vsTextLines.GetLineText(index, 0, index, length, out var result);
            return result;
        }

        internal static int LineEnd(this IVsTextLines vsTextLines, int line)
            => LinePosition(vsTextLines, line) + LineLength(vsTextLines, line);

        internal static TokenTriggers ConvertToTokenTrigger(this Item token)
        {
            var result = TokenTriggers.None;
            if(token.IsBraceLike)
                result |= TokenTriggers.MatchBraces;
            return result;
        }

        internal static TokenType ConvertToTokenType(this Item token)
        {
            if(token.IsComment)
                return TokenType.Comment;
            if(token.IsSpace || token.IsLineEnd)
                return TokenType.WhiteSpace;
            if(token.IsError)
                return TokenType.Unknown;
            if(token.IsText)
                return TokenType.String;
            if(token.IsNumber)
                return TokenType.Literal;
            if(token.IsKeyword)
                return TokenType.Keyword;
            if(token.IsIdentifier)
                return TokenType.Identifier;
            return TokenType.Text;
        }

        internal static TokenColor ConvertToTokenColor(this Item token)
        {
            if(token.IsComment )
                return TokenColor.Comment;
            if(token.IsKeyword)
                return TokenColor.Keyword;
            if(token.IsIdentifier)
                return TokenColor.Identifier;
            if(token.IsText)
                return TokenColor.String;
            if(token.IsNumber)
                return TokenColor.Number;

            return TokenColor.Text;
        }

        internal static IEnumerable<uint> SelectColors
            (this Item token, IEnumerable<char> toCharArray)
            => toCharArray
                .Select(c => (uint)token.ConvertToTokenColor());

        internal static TokenInfo ToTokenInfo(this Item token)
            => new TokenInfo
            {
                Color = token.ConvertToTokenColor(), Trigger = token.ConvertToTokenTrigger()
                , Type = token.ConvertToTokenType(), StartIndex = token.StartPosition, EndIndex = token.EndPosition
            };

        internal static TextSpan ToTextSpan(this SourcePart current)
            => new TextSpan
            {
                iStartLine = current.Start.LineIndex, iStartIndex = current.Start.ColumnIndex
                , iEndLine = current.End.LineIndex, iEndIndex = current.End.ColumnIndex
            };

        internal static Source CreateReniSource(this IVsTextLines buffer)
            => new Source(buffer.GetAll());

        internal static SourcePart ToSourcePart(this hw.Scanner.Source data, TextSpan span)
            => data.FromLineAndColumn(span.iStartLine, span.iStartIndex)
                .Span(data.FromLineAndColumn(span.iEndLine, span.iEndIndex));
    }
}