using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using hw.DebugFormatter;

namespace ReniBrowser.CompilationView
{
    sealed class EmptyTraceLogItem : DumpableObject, ITraceLogItem
    {
        Control ITraceLogItem.CreateLink() => "".CreateView();
    }
}