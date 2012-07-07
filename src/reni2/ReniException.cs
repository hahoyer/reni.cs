using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HWClassLibrary.Debug;

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
                _l.Add(Tracer.MethodHeader(i, FilePositionTag.Output, true));
            Tracer.FlaggedLine(text, FilePositionTag.Output);
            _text = text;
        }
    }
}