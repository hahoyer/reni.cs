using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using hw.Scanner;

namespace ReniUI.CompilationView
{
    abstract class ChildView : View
    {
        protected readonly SourceView Master;

        protected ChildView(SourceView master, string name)
            : base(name)
        {
            Master = master;
            Master.Register(Frame);
            Frame.Activated += (a, b) => Master.SelectSource(Source);
            Frame.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            Frame.MaximizeBox = false;
            Frame.MinimizeBox = false;
        }

        protected abstract SourcePart Source { get; }

        internal void Run()
        {
            Frame.Show();
            Frame.BringToFront();
        }
    }
}