using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;

namespace ReniBrowser.CompilationView
{
    sealed class StepView : ChildView
    {
        internal StepView(BrowseTraceCollector.Step item, SourceView master)
            : base(master, "Step " + item.Index + ": "+ item.CodeBase.GetIdText())
        {
            Client = item.CreateView(Master);
        }

        protected override SourcePart Source => null;
    }
}