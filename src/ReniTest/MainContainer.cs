using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using HWClassLibrary.Helper;
using HWClassLibrary.IO;
using HWClassLibrary.TreeStructure;
using HWClassLibrary.UnitTest;
using Reni;
using Reni.FeatureTest;
using Reni.FeatureTest.Function;
using Reni.FeatureTest.Integer;

namespace ReniTest
{
    public static class MainContainer
    {

        private const string Target = @"! property x: 11/\; x dump_print";
        const string Output = "11";

        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            //TestGenerated.Exec();
            //CompilerTest.Run("Test", Target, Output);
            //Assembly.GetExecutingAssembly().RunTests();
            //InspectCompiler();
            RunSpecificTest();
        }

        [Test, Category(CompilerTest.UnderConstruction)]
        private static void RunSpecificTest()
        {
            new TwoFunctions().RunFlat();
            new IntegerPlusNumber().RunFlat();
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