using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.Context;

namespace ReniBrowser.CompilationView
{
    sealed class ContextView : ChildView
    {
        public ContextView(ContextBase item, SourceView master)
            : base(master, "Context: "+ item.NodeDump)
        {
            Client = item.CreateView(Master);
            Source = item.GetSource();
        }

        protected override SourcePart Source { get; }
    }
}