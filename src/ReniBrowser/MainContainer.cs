using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Reni.FeatureTest;
using Reni.FeatureTest.Helper;
using ReniUI.CompilationView;

namespace reniBrowser
{
    static class MainContainer
    {
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            BrowseTestResult<TextConcat>();
        }

        static void BrowseTestResult<T>()
            where T : CompilerTest, new()
            => new SourceView(new T().Targets.First()).Run();
    }
}