using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
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
        static Dictionary<System.Type, CompilerTest> _cache;
        bool _needToRunDependants = true;

        public CompilerTest()
        {
            Parameters = new CompilerParameters();
            if(TestRunner.IsModeErrorFocus)
                Parameters.TraceOptions.UseOnModeErrorFocus();
        }

        internal void CreateFileAndRunCompiler
            (
            string name,
            string text,
            string expectedOutput = null,
            Action<Compiler> expectedResult = null)
            =>
                CreateFileAndRunCompiler
                    (name, new TargetSetData(text, expectedOutput), expectedResult);

        void CreateFileAndRunCompiler
            (string name, TargetSetData targetSetData, Action<Compiler> expectedResult)
        {
            var fileName = name + ".reni";
            var f = fileName.FileHandle();
            f.String = targetSetData.Target;
            InternalRunCompiler(fileName, expectedResult, targetSetData);
        }

        void InternalRunCompiler
            (string fileName, Action<Compiler> expectedResult, TargetSetData targetSet)
            => InternalRunCompiler(Parameters, fileName, expectedResult, targetSet);

        void InternalRunCompiler
            (
            CompilerParameters compilerParameters,
            string fileName,
            Action<Compiler> expectedResult,
            TargetSetData targetSet)
        {
            var outStream = new OutStream();
            compilerParameters.OutStream = outStream;
            var c = new Compiler(fileName, compilerParameters, "Reni");

            if(expectedResult != null)
            {
                c.Materialize();
                expectedResult(c);
            }

            if(Parameters.TraceOptions.ReformatedSource)
            {
                Tracer.Line
                    (
                        "---------------------\n" +
                            c.SourceSyntax.Reformat() +
                            "\n---------------------"
                    );
                Tracer.ConditionalBreak(true);
            }

            c.Execute();

            if(outStream.Data != targetSet.Output)
            {
                Tracer.Line("---------------------\n" + outStream.Data + "\n---------------------");
                Tracer.ThrowAssertionFailed
                    (
                        "outStream.Data != targetSet.Output",
                        () => "outStream.Data:" + outStream.Data + " expected: " + targetSet.Output);
            }

            try
            {
                Verify(c.Issues);
            }
            catch(Exception)
            {
                Tracer.Line("---------------------\n" + c.Issues + "\n---------------------");
                throw;
            }
        }

        void RunDependant()
        {
            RunDependants();
            Run();
        }

        public virtual void Run() => BaseRun(1);

        protected void BaseRun(int depth = 0)
        {
            if(_cache == null)
                _cache = new Dictionary<System.Type, CompilerTest>();

            foreach(var tuple in TargetSet)
                CreateFileAndRunCompiler(GetType().PrettyName(), tuple, AssertValid);
        }


        void RunDependants()
        {
            if(!_needToRunDependants)
                return;

            _needToRunDependants = false;

            if(_cache.ContainsKey(GetType()))
                return;

            _cache.Add(GetType(), this);

            foreach(
                var dependsOnType in
                    DependsOn.Where(dependsOnType => !_cache.ContainsKey(dependsOnType)))
                ((CompilerTest) Activator.CreateInstance(dependsOnType)).RunDependant();
        }

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
        protected virtual void Verify(IEnumerable<Issue> issues) => Tracer.Assert(!issues.Any());

        protected virtual IEnumerable<System.Type> DependsOn
        {
            get
            {
                return GetType()
                    .GetCustomAttributes(typeof(CompilerTest), true)
                    .Select(o => o.GetType());
            }
        }

        internal string GetStringAttribute<T>() where T : StringAttribute
        {
            var result = GetType().GetAttribute<T>(true);
            return result == null ? "" : result.Value;
        }

        protected virtual void AssertValid(Compiler c) { }
    }
}