using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using hw.DebugFormatter;
using ReniUI;

namespace ReniStudio
{
    sealed class Application : ApplicationContext, IStudioApplication
    {
        readonly List<Form> Children = new List<Form>();

        void IApplication.Register(Form child)
        {
            Children.Add(child);
            child.FormClosing += (a, s) => CheckedExit(child);
        }

        void IStudioApplication.Exit() => ExitThread();

        void IStudioApplication.New() => Dumpable.NotImplementedFunction();

        void CheckedExit(Form child)
        {
            Children.Remove(child);
            if(Children.Count == 0)
                ExitThread();
        }
    }
}