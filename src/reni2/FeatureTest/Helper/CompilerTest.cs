using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;

using hw.Helper;
using hw.UnitTest;
using Reni.Runtime;
using Reni.Validation;

namespace Reni.FeatureTest.Helper
{
    /// <summary>
    ///     Helper class for unittests, that compile something
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public abstract class CompilerTest : DependantAttribute, ITestFixture
    {
        internal readonly CompilerParameters Parameters;

        protected CompilerTest()
        {
            Parameters = new CompilerParameters();
            if(TestRunner.IsModeErrorFocus)
                Parameters.TraceOptions.UseOnModeErrorFocus();
        }

        internal Compiler CreateFileAndRunCompiler
            (
            string name,
            string text,
            string expectedOutput = null,
            Action<Compiler> expectedResult = null)
            =>
                CreateFileAndRunCompiler
                    (name, new TargetSetData(text, expectedOutput), expectedResult);

        Compiler CreateFileAndRunCompiler
            (string name, TargetSetData targetSetData, Action<Compiler> expectedResult)
        {
            var fileName = name + ".reni";
            var f = fileName.FileHandle();
            f.String = targetSetData.Target;
            return InternalRunCompiler(fileName, expectedResult, targetSetData);
        }

        Compiler InternalRunCompiler
            (string fileName, Action<Compiler> expectedResult, TargetSetData targetSet)
            => InternalRunCompiler(Parameters, fileName, expectedResult, targetSet);

        Compiler InternalRunCompiler
            (
            CompilerParameters compilerParameters,
            string fileName,
            Action<Compiler> expectedResult,
            TargetSetData targetSet)
        {
            var outStream = new OutStream();
            compilerParameters.OutStream = outStream;
            var compiler = Compiler.FromFile(fileName, compilerParameters);

            try
            {
                if(expectedResult != null)
                {
                    compiler.Materialize();
                    expectedResult(compiler);
                }

                compiler.Execute();

                if(outStream.Data != targetSet.Output)
                {
                    Tracer.Line
                        ("---------------------\n" + outStream.Data + "\n---------------------");
                    Tracer.ThrowAssertionFailed
                        (
                            "outStream.Data != targetSet.Output",
                            () =>
                                "outStream.Data:" + outStream.Data + " expected: "
                                    + targetSet.Output);
                }

                try
                {
                    Verify(compiler.Issues);
                }
                catch(Exception)
                {
                    Tracer.Line
                        ("---------------------\n" + compiler.Issues + "\n---------------------");
                    throw;
                }

                return compiler;
            }
            catch(Exception exception)
            {
                throw new RunException(exception, compiler);
            }
        }

        sealed class RunException : Exception
        {
            [Node]
            internal readonly Exception Exception;
            [Node]
            readonly Compiler Compiler;

            public RunException(Exception exception, Compiler compiler)
            {
                Exception = exception;
                Compiler = compiler;
            }
        }

        public virtual void Run()
        {
            try
            {
                BaseRun().ToArray();
            }
            catch(RunException runException)
            {
                throw runException.Exception;
            }
        }

        protected IEnumerable<Compiler> BaseRun()
        {
            return TargetSet
                .Select
                (tuple => CreateFileAndRunCompiler(GetType().PrettyName(), tuple, AssertValid));
        }

        public IEnumerable<Compiler> Inspect() => BaseRun();

        TargetSetData[] TargetSet
        {
            get
            {
                var result = GetType()
                    .GetAttributes<TargetSetAttribute>(true)
                    .Select(tsa => tsa.TargetSet)
                    .ToArray();

                if(Target == "")
                    return result;

                return result
                    .Concat(new[] {new TargetSetData(Target, Output)})
                    .ToArray();
            }
        }

        protected virtual string Output => GetStringAttribute<OutputAttribute>();
        protected virtual string Target => GetStringAttribute<TargetAttribute>();

        public string[] Targets => TargetSet.Select(item => item.Target).ToArray();

        protected virtual void Verify(IEnumerable<Issue> issues) => Tracer.Assert(!issues.Any());

        internal string GetStringAttribute<T>() where T : StringAttribute
        {
            var result = GetType().GetAttribute<T>(true);
            return result == null ? "" : result.Value;
        }

        protected virtual void AssertValid(Compiler c) { }
    }
}