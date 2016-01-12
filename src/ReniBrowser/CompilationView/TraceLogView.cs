using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using hw.Scanner;

namespace ReniBrowser.CompilationView
{
    sealed class TraceLogView : ChildView
    {
        readonly DataGridView LogView;

        public TraceLogView(SourceView master)
            : base(master, "TraceLogView")
        {
            master.RunCode();
            LogView = master.CreateTraceLogView();
            Client = LogView;
        }

        protected override SourcePart Source => null;

        public void SignalClickedObject(BrowseTraceCollector.Step[] target)
        {
            foreach(var item in target)
                LogView.Rows[item.Index].Selected = true;
        }
    }
}