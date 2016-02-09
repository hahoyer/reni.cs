using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using Reni.Struct;

namespace ReniBrowser.CompilationView
{
    sealed class FrameItem : DumpableObject
    {
        public FunctionId FunctionId;
        public string Text;
        public ITraceLogItem CallStep;
    }
}