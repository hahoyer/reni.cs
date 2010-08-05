using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using HWClassLibrary.IO;
using HWClassLibrary.TreeStructure;
using HWClassLibrary.UnitTest;
using Reni;
using Reni.FeatureTest;

namespace ReniTest
{
    public static class MainContainer
    {

        private const string Target = @"
f: {1000 type({arg = 1 then 1 else {arg * f[arg type((arg-1)enable_cut)]}}enable_cut)}/\;
f(4)dump_print
";
        const string Output = "24";

        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //TestGenerated.Exec();
            //CompilerTest.Run("Test", Target, Output);
            Assembly.GetExecutingAssembly().RunTests();
            //InspectCompiler();
        }

        private static void InspectCompiler()
        {
            Application.Run
                (
                    new TreeForm
                        {
                            Target = CreateCompiler(Target)
                        }
                );
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