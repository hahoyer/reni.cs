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
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            new EditorView("test.reni").Run();
        }
    }
}