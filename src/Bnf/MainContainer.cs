using System.Diagnostics;
using System.Reflection;
using hw.DebugFormatter;
using hw.Helper;
using hw.UnitTest;

namespace Bnf
{
    static class MainContainer
    {
        public static void Main()
        {
            new Tests.Test().TestMethod();
            RunAllTests();
            //ExecT4CompilerGeneratedTest();
        }

        static void RunAllTests()
        {
            if(Debugger.IsAttached)
                TestRunner.IsModeErrorFocus = true;
            Assembly.GetExecutingAssembly().RunTests();
        }

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
    }

    public sealed class OutStream : DumpableObject, IOutStream
    {
        void IOutStream.AddData(string x)
        {
            Data += x;
            Tracer.Line("-data----------------\n" + Data + "|<--\n---------------------");
            Tracer.Assert(Data.Length < 1000);
        }

        void IOutStream.AddLog(string x)
        {
            Log += x;
            Tracer.Line("-log----------------\n" + Log + "\n---------------------");
        }

        internal string Data {get; set;} = "";

        internal string Log {get; set;} = "";
    }

    public interface IOutStream
    {
        void AddData(string x);
        void AddLog(string x);
    }

    class Data
    {
        public static OutStream OutStream;
    }
}