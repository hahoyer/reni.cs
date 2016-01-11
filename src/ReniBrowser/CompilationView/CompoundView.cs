using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.Struct;

namespace ReniTest.CompilationView
{
    sealed class CompoundView : ChildView
    {
        public CompoundView(Compound item, SourceView master)
            : base(master, "Compound: " + item.NodeDump)
        {
            Client = item.CreateView(Master);
            Source = item.GetSource();
        }

        protected override SourcePart Source { get; }
    }
}