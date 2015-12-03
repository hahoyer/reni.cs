using System;
using System.Collections.Generic;
using System.Linq;

namespace ReniTest.CompilationView
{
    abstract class ChildView : View
    {
        protected readonly MainView Master;

        protected ChildView(MainView master, string name)
            : base(name) { Master = master; Master.Register(Frame);}

        internal void Run()
        {
            Frame.Show();
            Frame.BringToFront();
        }

    }
}