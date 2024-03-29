using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;

namespace ReniUI.CompilationView
{
    sealed class StepView : ChildView
    {
        readonly int Index;

        internal StepView(Step item, SourceView master)
            : base(master, "Step " + item.Index + ": "+ item.CodeBase.GetIdText(), "Step"+item.Index)
        {
            Index = item.Index;
            Client = item.CreateView(Master);
        }

        protected override SourcePart[] SourceParts => null;
    }
}