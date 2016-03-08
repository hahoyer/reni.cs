using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ReniUI;

namespace ReniStudio
{
    static class MainContainer
    {
        public static void Main()
        {
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
            var master = new Application();

            new EditorView("test.reni", master).Run();
            System.Windows.Forms.Application.Run(master);
        }
    }
}