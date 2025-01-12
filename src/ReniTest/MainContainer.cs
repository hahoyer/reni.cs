using System.Diagnostics;
using System.Reflection;
using hw.UnitTest;
using Reni;
using Reni.FeatureTest.Helper;
using Reni.Runtime;
using ReniUI;

namespace ReniTest;

static class MainContainer
{
    [UsedImplicitly]
    const string Target = @"f: @ ^(); x: 1; f(@x) dump_print";

    [UsedImplicitly]
    const string Output = "1";

    public static void Main()
    {
        "Start".Log();
        if(DateTime.Now.Year == 1)
            TestRuntime();
        RunAllTests();
    }

    static void RunAllTests()
    {
        var configuration = TestRunner.Configuration;

        configuration.IsBreakEnabled = Debugger.IsAttached;
        configuration.SaveResults = true;

        if(Debugger.IsAttached)
        {
            configuration.SkipSuccessfulMethods = true;
            configuration.SaveResults = false;
            PendingTests.Run();
        }

        configuration.TestsFileName = (SmbFile.SourceFolder / "PendingTests.cs").FullName;
        Assembly.GetExecutingAssembly().RunTests();
    }

    // Keep this to ensure reference to ReniUI
    [UsedImplicitly]
    static void BrowseCompiler(CompilerBrowser compiler) { }

    [UsedImplicitly]
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

    [UsedImplicitly]
    static void Run<TTarget>()
        where TTarget : CompilerTest, new()
        => new TTarget().Run();

    static void TestRuntime()
    {
        Data.OutStream = new OutStream();
        TestRuntimeCode();
        Data.OutStream = null;
    }

    static void TestRuntimeCode()
    {
        var data = Data.Create(18);
        data.SizedPush(1, 10);
        data.SizedPush(1, 4);
        data.Push(data.Pointer(1));
        data.Push(data.Pointer(8));
        data.Assign(1);
        data.Drop(1);
        Data.PrintText("(");
        data.Push(data.Get(1, 0).BitCast(5).BitCast(8));
        data.Pull(1).PrintNumber();
        Data.PrintText(", ");
        Data.PrintText(")");
        data.Drop(1);
    }
}