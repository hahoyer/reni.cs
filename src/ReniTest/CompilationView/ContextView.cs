using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using hw.Scanner;
using Reni.Context;
using Reni.Struct;

namespace ReniTest.CompilationView
{
    sealed class ContextView : ChildView
    {
        public ContextView(ContextBase item, SourceView master)
            : base(master, item.NodeDump)
        {
            Client = item.CreateView(Master);
            Source = item.GetSource();
        }

        protected override SourcePart Source { get; }
    }
}