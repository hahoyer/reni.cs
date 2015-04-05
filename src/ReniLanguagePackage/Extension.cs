using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Scanner;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using Reni.UserInterface;

namespace HoyerWare.ReniLanguagePackage
{
    static class Extension
    {
        internal static int GetCaretPosition(this IVsTextView textView)
        {
            int piLine;
            int piColumn;
            var rc = textView.GetCaretPos(out piLine, out piColumn);
            Tracer.Assert(rc == VSConstants.S_OK);
            int virt;
            int position;
            rc = textView.GetNearestPosition(piLine, piColumn, out position, out virt);
            Tracer.Assert(rc == VSConstants.S_OK);
            return position;
        }

        internal static int GetLineCount(this IVsTextLines vsTextLines)
        {
            int result;
            vsTextLines.GetLineCount(out result);
            return result;
        }

        internal static int LinePosition(this IVsTextLines vsTextLines, int lineIndex)
        {
            int result;
            vsTextLines.GetPositionOfLine(lineIndex, out result);
            return result;
        }

        internal static int LineIndex(this IVsTextLines vsTextLines, int position)
        {
            int result;
            int column;
            vsTextLines.GetLineIndexOfPosition(position, out result, out column);
            return result;
        }

        internal static int LineLength(this IVsTextLines vsTextLines, int lineIndex)
        {
            int result;
            vsTextLines.GetLengthOfLine(lineIndex, out result);
            return result;
        }

        internal static string GetAll(this IVsTextLines vsTextLines)
        {
            int lineCount;
            vsTextLines.GetLineCount(out lineCount);
            int lengthOfLastLine;
            vsTextLines.GetLengthOfLine(lineCount - 1, out lengthOfLastLine);
            string result;
            vsTextLines.GetLineText(0, 0, lineCount - 1, lengthOfLastLine, out result);
            return result;
        }

        internal static string Line(IVsTextLines vsTextLines, int index)
        {
            int length;
            vsTextLines.GetLengthOfLine(index, out length);
            string result;
            vsTextLines.GetLineText(index, 0, index, length, out result);
            return result;
        }

        internal static int LineEnd(this IVsTextLines vsTextLines, int line)
            => LinePosition(vsTextLines, line) + LineLength(vsTextLines, line);

        internal static TokenTriggers ConvertToTokenTrigger(this TokenInformation token)
        {
            var result = TokenTriggers.None;
            if(token.IsBraceLike)
                result |= TokenTriggers.MatchBraces;
            return result;
        }
        internal static TokenType ConvertToTokenType(this TokenInformation token)
        {
            if(token.IsComment)
                return TokenType.Comment;
            if(token.IsLineComment)
                return TokenType.LineComment;
            if(token.IsWhiteSpace)
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

        internal static TokenColor ConvertToTokenColor(this TokenInformation token)
        {
            if(token.IsComment || token.IsLineComment)
                return TokenColor.Comment;
            if(token.IsError)
                return TokenColor.Text;
            if(token.IsText)
                return TokenColor.String;
            if(token.IsNumber)
                return TokenColor.Number;
            if(token.IsKeyword)
                return TokenColor.Keyword;
            if(token.IsIdentifier)
                return TokenColor.Identifier;

            return TokenColor.Text;
        }

        internal static IEnumerable<uint> SelectColors
            (this TokenInformation token, IEnumerable<char> toCharArray)
        {
            return toCharArray
                .Select(c => (uint) token.ConvertToTokenColor());
        }

        internal static TokenInfo ToTokenInfo(this TokenInformation token)
            => new TokenInfo
            {
                Color = token.ConvertToTokenColor(),
                Trigger = token.ConvertToTokenTrigger(),
                Type = token.ConvertToTokenType(),
                StartIndex = token.StartPosition,
                EndIndex = token.EndPosition
            };

        internal static TextSpan Span(this SourcePart current)
            => new TextSpan
            {
                iStartLine = current.Start.LineIndex,
                iStartIndex = current.Start.ColumnIndex,
                iEndLine = current.End.LineIndex,
                iEndIndex = current.End.ColumnIndex
            };

        internal static Source CreateReniSource(this IVsTextLines buffer)
            => new Source(buffer.GetAll());
    }
}