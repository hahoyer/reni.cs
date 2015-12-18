using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.Type;

namespace ReniTest.CompilationView
{
    sealed class TypeView : ChildView
    {
        public TypeView(TypeBase item, SourceView master)
            : base(master, "Type: " + item.NodeDump)
        {
            Client = item.CreateView(Master);
            Source = item.GetSource();
        }

        protected override SourcePart Source { get; }
    }
}