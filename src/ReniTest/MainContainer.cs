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
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            RunAllTests();
            //ExecT4CompilerGeneratedTest();
        }

        static void RunAllTests()
        {
            new ReniUI.Test.BraceMatching().MatchingBraces();

            if(Debugger.IsAttached)
            {
                TestRunner.IsBreakDisabled = false;
                TestRunner.IsModeErrorFocus = true;
                TestRunner.TestsFileName = SmbFile.SourcePath(0).PathCombine("PendingTests.debug.cs");
            }
            else
                TestRunner.TestsFileName = SmbFile.SourcePath(0).PathCombine("PendingTests.cs");

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