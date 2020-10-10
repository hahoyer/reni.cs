using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using hw.DebugFormatter;
using hw.Helper;
using hw.UnitTest;
using Reni;
using Reni.FeatureTest.Helper;
using Reni.Runtime;
using ReniUI;

namespace ReniTest
{
    static class MainContainer
    {
        const string Target = @"f: /\ ^(); x: 1; f(/\x) dump_print";
        const string Output = "1";

        public static void Main()
        {
            "Start".Log();
            Log4NetTextWriter.Register(false);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            RunAllTests();
            //ExecT4CompilerGeneratedTest();
        }

        static void RunAllTests()
        {
            var configuration = TestRunner.Configuration;

            configuration.IsBreakEnabled = Debugger.IsAttached;
            configuration.SaveResults = true;
            
            if(Debugger.IsAttached)
            {
                configuration.SkipSuccessfulMethods = true;
                PendingTests.Run();
            }

            configuration.TestsFileName = SmbFile.SourcePath().PathCombine("PendingTests.cs");
            Assembly.GetExecutingAssembly().RunTests();

        }

        // Keep this to ensure reference to ReniUI
        static void BrowseCompiler(CompilerBrowser compiler) { }

        static Compiler CreateCompiler(string text)
        {
            Tracer.IsBreakDisabled = false;
            const string fileName = "temptest.reni";
            var f = fileName.ToSmbFile();
            f.String = text;
            var compiler = Compiler.FromText(fileName);
            //Profiler.Measure(()=>compiler.Exec());
            //Tracer.FlaggedLine(Profiler.Format(10,0.0));
            return compiler;
        }

        static void ExecT4CompilerGeneratedTest()
        {
            Data.OutStream = new OutStream();
            T4CompilerGenerated.MainFunction();
            Data.OutStream = null;
        }

        static void Run<TTarget>()
            where TTarget : CompilerTest, new()
            => new TTarget().Run();
    }
}


