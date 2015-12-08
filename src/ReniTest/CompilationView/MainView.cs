using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Reni.Struct;

namespace ReniTest.CompilationView
{
    abstract class MainView : View
    {
        protected MainView(string name)
            : base(name) { }

        internal void Register(Form child) { Frame.Closing += (a, s) => child.Close(); }
        internal void Run() => Application.Run(Frame);

    }
}