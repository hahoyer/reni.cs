using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using hw.DebugFormatter;
using ReniUI;

namespace ReniStudio
{
    static class MainContainer
    {
        [STAThread]
        public static void Main()
        {
            //Tracer.IsBreakDisabled = true;
            new StudioApplication().Run();
        }
    }
}