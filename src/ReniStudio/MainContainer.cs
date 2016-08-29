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
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var master = new StudioApplication();
            master.Initialize();
            Application.Run(master);
        }
    }
}