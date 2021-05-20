using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using hw.DebugFormatter;

namespace ReniUI.CompilationView
{
    sealed class EmptyTraceLogItem : DumpableObject, ITraceLogItem
    {
        Control ITraceLogItem.CreateLink() => "".CreateView();
    }
}