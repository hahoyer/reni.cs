using System;
using System.Diagnostics;
using System.Reflection;
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

        [SetUp]
        public void Start()
        {
            Parameters = new CompilerParameters();
        }

        protected void RunCompiler(string name, string text, string expectedOutput)
        {
            RunCompiler(1, name, text, expectedOutput);
        }

        protected void RunCompiler(string name, string text, ExpectedResult expectedResult, string expectedOutput)
        {
            RunCompiler(1, name, text, expectedResult, expectedOutput);
        }

        protected void RunCompiler(string name, string text, ExpectedResult expectedResult)
        {
            RunCompiler(1, name, text, expectedResult, "");
        }

        protected void RunCompiler(string name, string expectedOutput)
        {
            RunCompiler(1, name, expectedOutput);
        }

        private void RunCompiler(int depth, string name, string text, ExpectedResult expectedResult,
            string expectedOutput)
        {
            var fileName = name + ".reni";
            var f = File.m(fileName);
            f.String = text;
            InternalRunCompiler(depth + 1, fileName, expectedResult, expectedOutput);
        }

        private void RunCompiler(int depth, string name, string text, string expectedOutput)
        {
            RunCompiler(depth + 1, name, text, default(ExpectedResult), expectedOutput);
        }

        private void RunCompiler(int depth, string name, string expectedOutput)
        {
            RunCompiler(depth + 1, name, default(ExpectedResult), expectedOutput);
        }

        private void RunCompiler(int depth, string name, ExpectedResult expectedResult, string expectedOutput)
        {
            InternalRunCompiler(depth + 1, File.SourcePath(1) + "\\" + name + ".reni", expectedResult, expectedOutput);
        }

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
                    {
                        if(((CategoryAttribute) xx[ii]).Name == UnderConstruction)
                            return true;
                    }
                    return false;
                }
            }
            return false;
        }

        private static MethodBase GetTestMethod(int depth)
        {
            for (var i = 0; i < 100; i++)
            {
                var result = new StackTrace(true).GetFrame(depth + i).GetMethod();
                if (result.GetCustomAttributes(typeof(TestAttribute), true).Length > 0)
                    return result;
            }
            return null;
        }

        protected void GenericRun()
        {
            GenericRun(0);
            
        }

        private void GenericRun(int depth)
        {
            var m = GetTestMethod(depth+1);
            var n = m.Name.Split('_')[2];
            var type = FindNestedType(n);
            var testObject = (CompilerTestClass) Activator.CreateInstance(type);
            testObject.Start();
            testObject.Run();
        }
        private System.Type FindNestedType(string name)
        {
            var types = GetType().GetNestedTypes();
            foreach (var type in types)
            {
                if(type.Name == name)
                    return type;
            }
            return null;
        }
    }

    public abstract class CompilerTestClass: CompilerTest
    {
        public virtual void Run()
        {
            RunCompiler(GetType().Name, Target, Output);
        }

        public abstract string Output { get; }
        public abstract string Target { get; }
    }
}