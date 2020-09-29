using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.Type;

namespace ReniUI.CompilationView
{
    sealed class TypeView : ChildView
    {
        internal TypeView(TypeBase item, SourceView master)
            : base(master, "Type: " + item.NodeDump)
        {
            Client = item.CreateView(Master);
            SourcePart = item.GetSource();
        }

        protected override SourcePart SourcePart { get; }
    }
}