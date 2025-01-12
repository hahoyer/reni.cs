using Microsoft.VisualStudio.TextManager.Interop;

namespace ReniVSIX;

static class Extension
{
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
}