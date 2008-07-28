using System;
using System.Collections.Generic;
using System.Diagnostics;
using HWClassLibrary.Debug;
using HWClassLibrary.IO;
using NUnit.Framework;

namespace Reni.FeatureTest
{
    /// <summary>
    /// Helper class for unittests, that compile something
    /// </summary>
    public abstract class CompilerTest
    {
        public delegate void ExpectedResult(Compiler c);

        public const string Damaged = "Damaged";
        public const string Rare = "Rare";
        public const string UnderConstruction = "Under Construction";
        public const string UnderConstructionNoAutoTrace = "Under Construction (No auto trace)";
        public const string Worked = "Worked";
        public CompilerParameters Parameters;
        private Dictionary<System.Type, CompilerTest> _cache;

        [SetUp]
        public void Start() { Parameters = new CompilerParameters(); }

        protected void RunCompiler(string name, string text, string expectedOutput) { RunCompiler(1, name, text, expectedOutput); }

        protected void RunCompiler(string name, string text, ExpectedResult expectedResult, string expectedOutput) { RunCompiler(1, name, text, expectedResult, expectedOutput); }

        protected void RunCompiler(string name, string text, ExpectedResult expectedResult) { RunCompiler(1, name, text, expectedResult, ""); }

        protected void RunCompiler(string name, string expectedOutput) { RunCompiler(1, name, expectedOutput); }

        private void RunCompiler(int depth, string name, string text, ExpectedResult expectedResult,
            string expectedOutput)
        {
            var fileName = name + ".reni";
            var f = File.m(fileName);
            f.String = text;
            InternalRunCompiler(depth + 1, fileName, expectedResult, expectedOutput);
        }

        internal void RunCompiler(int depth, string name, string text, string expectedOutput) { RunCompiler(depth + 1, name, text, default(ExpectedResult), expectedOutput); }

        private void RunCompiler(int depth, string name, string expectedOutput) { RunCompiler(depth + 1, name, default(ExpectedResult), expectedOutput); }

        private void RunCompiler(int depth, string name, ExpectedResult expectedResult, string expectedOutput) { InternalRunCompiler(depth + 1, File.SourcePath(1) + "\\" + name + ".reni", expectedResult, expectedOutput); }

        private void InternalRunCompiler(int depth, string fileName, ExpectedResult expectedResult,
            string expectedOutput)
        {
            Tracer.FlaggedLine(depth + 1, "Position of method tested");
            if(IsCallerUnderConstruction(1))
                Parameters.Trace.All();

            var c = new Compiler(Parameters, fileName);

            if(expectedResult != default(ExpectedResult))
            {
                c.Materialize();
                expectedResult(c);
            }

            var os = c.Exec();
            if(os != null && os.Data != expectedOutput)
            {
                os.Exec();
                Tracer.ThrowAssertionFailed(
                    "os.Data != expectedOutput",
                    "os.Data:" + os.Data + " expected: " + expectedOutput);
            }
        }

        private static bool IsCallerUnderConstruction(int depth)
        {
            for(var i = 0; i < 100; i++)
            {
                var x = new StackTrace(true).GetFrame(depth + i).GetMethod();
                if(x.GetCustomAttributes(typeof(TestAttribute), true).Length > 0)
                {
                    var xx = x.GetCustomAttributes(typeof(CategoryAttribute), true);
                    for(var ii = 0; ii < xx.Length; ii++)
                        if(((CategoryAttribute) xx[ii]).Name == UnderConstruction)
                            return true;
                    return false;
                }
            }
            return false;
        }

        private void RunDependant(Dictionary<System.Type, CompilerTest> cache)
        {
            _cache = cache;
            RunDependants();
            Start();
            Run();
        }

        public abstract void Run();

        protected void BaseRun()
        {
            if(_cache == null)
            {
                _cache = new Dictionary<System.Type, CompilerTest>();
                RunDependants();
            }

            RunCompiler(1, GetType().Name, Target, AssertValid, Output);
        }

        private void RunDependants()
        {
            _cache.Add(GetType(), this);
            foreach(var dependsOnType in DependsOn)
                if(!_cache.ContainsKey(dependsOnType))
                    ((CompilerTest) Activator.CreateInstance(dependsOnType)).RunDependant(_cache);
        }

        public virtual string Output { get { return ""; } }
        public virtual string Target { get { return ""; } }
        public virtual System.Type[] DependsOn { get { return new System.Type[0]; } }
        public virtual void AssertValid(Compiler c) {}
    }
}