using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using HWClassLibrary.Debug;
using HWClassLibrary.IO;
using HWClassLibrary.TreeStructure;
using HWClassLibrary.UnitTest;
using Reni;
using Reni.FeatureTest;
using Reni.FeatureTest.Function;

namespace ReniTest
{
    internal static class MainContainer
    {
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //TestGenerated.Exec();

            //CompilerTest.Run("Test", "(-1234) dump_print", "-1234");
            //RunSpecificTest();
            //TestRunner.IsModeErrorFocus = true;
            //Assembly.GetExecutingAssembly().RunTests();
            //InspectCompiler();
            Reni.Proof.Main.Run();
        }

        [Test, Category(CompilerTest.UnderConstruction)]
        private static void RunSpecificTest() { new FunctionWithNonLocal().Run(); }

        private const string Target = @"! property x: 11/\; x dump_print";
        private const string Output = "11";
        private static void InspectCompiler() { Application.Run(new TreeForm {Target = CreateCompiler(Target)}); }

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