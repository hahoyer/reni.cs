using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.Code;

namespace ReniUI.CompilationView
{
    sealed class CodeView : ChildView
    {
        public CodeView(CodeBase item, SourceView master)
            : base(master, "Code: " + item.NodeDump)
        {
            Client = item.CreateView(Master);
            Source = item.GetSource();
        }

        protected override SourcePart Source { get; }
    }
}