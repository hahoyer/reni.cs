using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using hw.DebugFormatter;
using hw.Helper;
using hw.UnitTest;
using Reni;
using Reni.FeatureTest.TypeType;
using Reni.Runtime;
using ReniUI;

namespace ReniTest
{
    static class MainContainer
    {
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            RunAllTests();
            //ExecT4CompilerGeneratedTest();
        }

        static void RunAllTests()
        {
            if(Debugger.IsAttached)
                TestRunner.IsModeErrorFocus = true;
            System.Reflection.Assembly.GetExecutingAssembly().RunTests();
        }

        const string Target = @"f: /\ ^(); x: 1; f(/\x) dump_print";
        const string Output = "1";

        // Keep this to ensure reference to ReniUI
        static void BrowseCompiler(CompilerBrowser compiler) { }

        static Compiler CreateCompiler(string text)
        {
            Tracer.IsBreakDisabled = false;
            const string fileName = "temptest.reni";
            var f = fileName.FileHandle();
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

        [UnitTest]
        public sealed class TypeNameExtenderTest
        {
            [UnitTest]
            public void TestMethod()
            {
                InternalTest(typeof(int), "int");
                InternalTest(typeof(List<int>), "List<int>");
                InternalTest(typeof(List<List<int>>), "List<List<int>>");
                InternalTest(typeof(Dictionary<int, string>), "Dictionary<int,string>");
                InternalTest(typeof(TypeOperator), "TypeType.TypeOperator");
            }

            [DebuggerHidden]
            static void InternalTest(Type type, string expectedTypeName)
            {
                Tracer
                    .Assert
                    (
                        type.PrettyName() == expectedTypeName,
                        () =>
                            type + "\nFound   : " + type.PrettyName() + "\nExpected: "
                                + expectedTypeName,
                        1);
            }
        }
    }
}