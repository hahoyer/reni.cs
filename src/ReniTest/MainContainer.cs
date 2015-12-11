using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using hw.Debug;
using hw.Forms;
using hw.Helper;
using hw.UnitTest;
using Reni;
using Reni.FeatureTest;
using Reni.FeatureTest.Helper;
using Reni.FeatureTest.TypeType;
using Reni.ParserTest;
using Reni.Runtime;
using ReniTest.CompilationView;

namespace ReniTest
{
    static class MainContainer
    {
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //if(false)
            //ExecTest();

            if(Debugger.IsAttached)
                TestRunner.IsModeErrorFocus = true;
            Assembly.GetExecutingAssembly().RunTests();
            //InspectCompiler(new TextConcat());
            //Reni.Proof.Main.Run();

            new SourceView(new TextConcat().Targets.First()).Run();
        }

        const string Target = @"f: /\ ^(); x: 1; f(/\x) dump_print";
        const string Output = "1";

        static void InspectCompiler(CompilerTest compiler)
        {
            object target = null;
            try
            {
                target = compiler.Inspect().ToArray();
            }
            catch(Exception exception)
            {
                target = exception;
            }

            Application.Run
                (
                    new TreeForm
                    {
                        Target = target
                    });
        }

        static void ShowSyntaxTree()
        {
            var prioTable = @"Left not
Left and
Left or
Left * /
Left + -
Left = <>
Right :=
TELevel then else
Left function
Right :
Right , ;
ParLevel ( { ) }
".FormatPrioTable();
            var image = prioTable.SyntaxGraph("(x a)(b)");
            var mainForm = new Form
            {
                ClientSize = image.Size,
                BackgroundImage = image,
                BackgroundImageLayout = ImageLayout.Stretch
            };

            Application.Run(mainForm);
        }

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

        static void ExecTest()
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