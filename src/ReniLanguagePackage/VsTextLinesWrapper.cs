using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Microsoft.VisualStudio.TextManager.Interop;

namespace HoyerWare.ReniLanguagePackage
{
    sealed class VsTextLinesWrapper : DumpableObject
    {
        public VsTextLinesWrapper(IVsTextLines data) { Data = data; }

        IVsTextLines Data { get; }

        internal int LineCount
        {
            get
            {
                int result;
                Data.GetLineCount(out result);
                return result;
            }
        }

        internal int LinePosition(int lineIndex)
        {
            int result;
            Data.GetPositionOfLine(lineIndex, out result);
            return result;
        }

        internal int LineIndex(int position)
        {
            int result;
            int column;
            Data.GetLineIndexOfPosition(position, out result, out column);
            return result;
        }

        internal int LineLength(int lineIndex)
        {
            int result;
            Data.GetLengthOfLine(lineIndex, out result);
            return result;
        }


        [DisableDump]
        internal string All
        {
            get
            {
                int lineCount;
                Data.GetLineCount(out lineCount);
                int lengthOfLastLine;
                Data.GetLengthOfLine(lineCount - 1, out lengthOfLastLine);
                string result;
                Data.GetLineText(0, 0, lineCount - 1, lengthOfLastLine, out result);
                return result;
            }
        }

        internal string Line(int index)
        {
            int length;
            Data.GetLengthOfLine(index, out length);
            string result;
            Data.GetLineText(index, 0, index, length, out result);
            return result;
        }

        internal int LineEnd(int line) => LinePosition(line) + LineLength(line);
    }
}