using System;
using System.Collections;
using System.Diagnostics;
using hw.DebugFormatter;

namespace Reni
{
    /// <summary>
    ///     Summary description for ReniException.
    /// </summary>
    public class ReniException : Exception
    {
        private readonly ArrayList _l = new ArrayList();
        private string _text;

        /// <summary>
        ///     ctor
        /// </summary>
        /// <param name = "text"></param>
        public ReniException(string text)
        {
            var st = new StackTrace(true);
            for(var i = 1; i < st.FrameCount; i++)
                _l.Add(Tracer.MethodHeader(tag: FilePositionTag.Output, showParam: true, stackFrameDepth: i));
            text.FlaggedLine(FilePositionTag.Output);
            _text = text;
        }
    }
}