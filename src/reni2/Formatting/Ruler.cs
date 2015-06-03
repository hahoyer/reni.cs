using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;

namespace Reni.Formatting
{
    sealed class Ruler : DumpableObject
    {
        readonly Frame _frame;
        [EnableDump]
        readonly bool _isMultiLine;

        public Ruler(Frame frame, bool isMultiLine)
        {
            _frame = frame;
            _isMultiLine = isMultiLine;
        }

        [EnableDump]
        string Id => _frame.Target.TokenClass.NodeDump();
    }
}