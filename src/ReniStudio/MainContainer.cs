using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ReniUI;

namespace ReniStudio
{
    static class MainContainer
    {
        [STAThread]
        public static void Main() => new StudioApplication().Run();
    }
}