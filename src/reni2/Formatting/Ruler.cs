using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;

namespace Reni.Formatting
{
    sealed class Ruler : DumpableObject
    {
        readonly Frame Frame;
        [EnableDump]
        readonly bool IsMultiLine;

        public Ruler(Frame frame, bool isMultiLine)
        {
            Frame = frame;
            IsMultiLine = isMultiLine;
        }

        [EnableDump]
        string Id => Frame.Target.TokenClass.NodeDump();
    }
}