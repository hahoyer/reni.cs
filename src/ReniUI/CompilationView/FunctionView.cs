using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.Struct;

namespace ReniUI.CompilationView
{
    sealed class FunctionView : ChildView
    {
        public FunctionView(FunctionType item, SourceView master)
            : base(master, "Function: " + item.NodeDump)
        {
            Client = item.CreateView(Master);
            Source = item.Body.SourcePart;
        }

        protected override SourcePart Source { get; }
    }
}