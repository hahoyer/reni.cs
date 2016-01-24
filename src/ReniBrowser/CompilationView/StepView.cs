using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;

namespace ReniBrowser.CompilationView
{
    sealed class StepView : ChildView
    {
        readonly int Index;

        internal StepView(BrowseTraceCollector.Step item, SourceView master)
            : base(master, "Step " + item.Index + ": "+ item.CodeBase.GetIdText())
        {
            Index = item.Index;
            Client = item.CreateView(Master);
        }

        protected override SourcePart Source => null;
        protected override string GetFileName() => "Step"+Index;
    }
}