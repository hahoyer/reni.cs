using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ReniUI
{
    public abstract class MainView : View
    {
        protected MainView(string name, string configFileName = null)
            : base(name, configFileName) { Frame.FormClosing += (a, b) => Application.Exit(); }

        internal void Register(Form child) { Frame.Closing += (a, s) => child.Close(); }
        internal void Run() => Application.Run(Frame);
    }
}