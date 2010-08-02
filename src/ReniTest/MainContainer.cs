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
using Reni.FeatureTest.Integer;

namespace ReniTest
{
    public static class MainContainer
    {
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //InspectCompiler();
            CompilerTest.Run("Test", @"
x: 100;
f1: ((
  y:3;
  f: x+3/\;
  f()
) _A_T_ 2)/\;

f1()dump_print;
", "103");
            CompilerTest.Run("Test", @"
x: 100;
f1: ((
  y: 3;
  f: arg*y+x/\;
  f(2)
) _A_T_ 2)/\;

f1()dump_print;
", "106");
            Assembly.GetExecutingAssembly().RunTests();
        }

        private static void InspectCompiler()
        {
            Application.Run
                (
                    new TreeForm
                        {
                            Target = CreateCompiler(new IntegerPlusNumber().Target)
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