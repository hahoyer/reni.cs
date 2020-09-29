using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;

namespace ReniUI.CompilationView
{
    abstract class ChildView : ReniUI.ChildView
    {
        protected readonly SourceView Master;

        protected ChildView(SourceView master, string name, string configFileName = null)
            : base(master, configFileName)
        {
            Title = name;
            Master = master;
            Frame.Activated += (a, b) => Master.SelectSource(SourcePart);
        }

        protected abstract SourcePart SourcePart { get; }
    }
}