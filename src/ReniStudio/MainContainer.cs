using System;
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