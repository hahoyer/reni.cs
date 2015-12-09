using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.Context;
using Reni.Struct;
using Reni.Type;

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

    sealed class ContextView : ChildView
    {
        public ContextView(ContextBase item, SourceView master)
            : base(master, item.NodeDump)
        {
            Client = item.CreateView(Master);
            Source = null;
        }

        protected override SourcePart Source { get; }
    }

    sealed class TypeView : ChildView
    {
        public TypeView(TypeBase item, SourceView master)
            : base(master, item.NodeDump)
        {
            Client = item.CreateView(Master);
            Source = null;
        }

        protected override SourcePart Source { get; }
    }
}