using hw.UnitTest;
using Reni.Runtime;
using Reni.Validation;

namespace Reni.FeatureTest.Helper;

/// <summary>
///     Helper class for unit tests, that compile something
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
[PublicAPI]
public abstract class CompilerTest : DependenceProvider, ITestFixture
{
    sealed class RunException : Exception
    {
        [Node]
        internal readonly Exception Exception;

        [Node]
        [UsedImplicitly]
        readonly Compiler Compiler;

        public RunException(Exception exception, Compiler compiler)
        {
            Exception = exception;
            Compiler = compiler;
        }
    }

    internal readonly CompilerParameters Parameters;

    [UsedImplicitly]
    Compiler[]? RunResults;

    TargetSetData[] TargetSet
    {
        get
        {
            var result = GetType()
                .GetAttributes<TargetSetAttribute>(true)
                .Select(tsa => tsa.TargetSet)
                .ToArray();

            if(Target == null || Target == "")
                return result;

            return result
                .Concat([new(Target, Output)])
                .ToArray();
        }
    }

    public string[] Targets => TargetSet.Select(item => item.Target).ToArray();

    protected CompilerTest()
    {
        Parameters = new();
        if(TestRunner.Configuration.SkipSuccessfulMethods)
            Parameters.TraceOptions.UseOnModeErrorFocus();
    }

    public virtual void Run()
    {
        try
        {
            RunResults = BaseRun().ToArray();
        }
        catch(RunException runException)
        {
            throw runException.Exception;
        }
    }

    protected virtual string? Output => GetStringAttribute<OutputAttribute>();
    protected virtual string? Target => GetStringAttribute<TargetAttribute>();

    protected virtual void Verify(IEnumerable<Issue> issues)
        => (!issues.Any()).Assert(() => issues.Select(issue => issue.LogDump).Stringify("\n"));

    protected virtual void AssertValid(Compiler c) { }

    internal Compiler CreateFileAndRunCompiler
    (
        string name,
        string text,
        string? expectedOutput = null,
        Action<Compiler>? expectedResult = null
    )
        => CreateFileAndRunCompiler(name, new(text, expectedOutput), expectedResult);

    internal Compiler GetFilesAndRunCompiler
        (string[] names, string? expectedOutput = null, Action<Compiler>? expectedResult = null)
    {
        var fileNames = names.Select(name => name + ".reni").ToArray();
        return RunCompiler(fileNames, expectedOutput, expectedResult);
    }


    Compiler CreateFileAndRunCompiler(string name, TargetSetData targetSetData, Action<Compiler>? expectedResult)
    {
        var fileName = name + ".reni";
        var f = fileName.ToSmbFile();
        f.String = targetSetData.Target;
        return RunCompiler(fileName, expectedResult, targetSetData);
    }

    Compiler RunCompiler(string fileName, Action<Compiler>? expectedResult, TargetSetData targetSet)
        => RunCompiler(Compiler.FromFile(fileName, Parameters), targetSet.Output, expectedResult);

    Compiler RunCompiler(string[] fileNames, string? expectedOutput, Action<Compiler>? expectedResult)
        => RunCompiler(Compiler.FromFiles(fileNames, Parameters), expectedOutput, expectedResult);

    Compiler RunCompiler(Compiler compiler, string? expectedOutput, Action<Compiler>? expectedResult)
    {
        var outStream = new OutStream();
        Parameters.OutStream = outStream;

        try
        {
            if(expectedResult != null)
            {
                compiler.Materialize();
                expectedResult(compiler);
            }

            compiler.Execute();

            if(expectedOutput != null)
            {
                var result = outStream.Data;
                if(result != expectedOutput)
                {
                    ("---------------------\n" + result + "\n---------------------").Log();
                    Tracer.ThrowAssertionFailed
                    (
                        "outStream.Data != expected",
                        () => "outStream.Data:" + result + " expected: " + expectedOutput
                    );
                }
            }

            foreach(var issue in compiler.Issues)
                issue.Dump().Log();

            try
            {
                Verify(compiler.Issues);
            }
            catch(Exception)
            {
                ("---------------------\n" + compiler.Issues + "\n---------------------").Log();
                throw;
            }

            return compiler;
        }
        catch(Exception exception)
        {
            throw new RunException(exception, compiler);
        }
    }

    protected IEnumerable<Compiler> BaseRun() => TargetSet
        .Select(tuple => CreateFileAndRunCompiler(GetType().PrettyName(), tuple, AssertValid));

    public IEnumerable<Compiler> Inspect() => BaseRun();

    internal string? GetStringAttribute<T>()
        where T : StringAttribute
    {
        var result = GetType().GetAttribute<T>(true);
        return result == null? "" : result.Value;
    }
}