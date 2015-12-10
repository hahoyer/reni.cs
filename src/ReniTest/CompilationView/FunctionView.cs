using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.Struct;

namespace ReniTest.CompilationView
{
    sealed class FunctionView : ChildView
    {
        public FunctionView(FunctionType item, SourceView master)
            : base(master, item.NodeDump)
        {
            Client = item.CreateView(Master);
            Source = item.Body.SourcePart;
        }

        protected override SourcePart Source { get; }
    }
}