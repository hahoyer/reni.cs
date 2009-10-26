using System;
using System.Windows.Forms;
using HWClassLibrary.Debug;
using HWClassLibrary.IO;
using HWClassLibrary.TreeStructure;
using Reni;
using Reni.FeatureTest.Integer;

namespace ReniTest
{
    public static class MainContainer
    {
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TreeForm { Target = CreateCompiler(new Plus().Target) });
        }

        private static Compiler CreateCompiler(string text)
        {
            const string fileName = "temptest.reni";
            var f = File.m(fileName);
            f.String = text;
            var compiler = new Compiler(fileName);
            //Profiler.Measure(()=>compiler.Exec());
            //Tracer.FlaggedLine(Profiler.Format(10,0.0));
            return compiler;
        }
    }
}