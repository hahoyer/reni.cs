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
        #region Delegates

        public delegate void ExpectedResult(Compiler c);

        #endregion

        /// <summary>
        /// NUnit category flag for test worked once
        /// </summary>
        public const string Damaged = "Damaged";

        /// <summary>
        /// NUnit category flag for test that are used rarely. Normally special preparations are required too.
        /// </summary>
        public const string Rare = "Rare";

        /// <summary>
        /// NUnit category flag for test that never worked
        /// </summary>
        public const string UnderConstruction = "Under Construction";

        /// <summary>
        /// NUnit category flag for test that never worked
        /// </summary>
        public const string UnderConstructionNoAutoTrace = "Under Construction (No auto trace)";

        /// <summary>
        /// NUnit category flag for test worked once
        /// </summary>
        public const string Worked = "Worked";

        /// <summary>
        /// Compiler parameter
        /// </summary>
        public CompilerParameters Parameters;

        /// <summary>
        /// Starts this instance.
        /// </summary>
        /// created 10.01.2007 04:32
        [SetUp]
        public void Start()
        {
            Parameters = new CompilerParameters();
        }

        /// <summary>
        /// Runs the compiler.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="text">The text.</param>
        /// <param name="expectedOutput">The expeced output.</param>
        /// created 17.11.2006 20:38
        protected void RunCompiler(string name, string text, string expectedOutput)
        {
            RunCompiler(1, name, text, expectedOutput);
        }

        /// <summary>
        /// Runs the compiler.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="text">The text.</param>
        /// <param name="expectedResult">The expected result.</param>
        /// <param name="expectedOutput">The expected output.</param>
        /// created 15.07.2007 23:47 on HAHOYER-DELL by hh
        protected void RunCompiler(string name, string text, ExpectedResult expectedResult, string expectedOutput)
        {
            RunCompiler(1, name, text, expectedResult, expectedOutput);
        }

        /// <summary>
        /// Runs the compiler.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="expectedOutput">The expected output.</param>
        /// created 15.07.2007 23:47 on HAHOYER-DELL by hh
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

        /// <summary>
        /// Runs the compiler.
        /// </summary>
        /// <param name="depth">The depth.</param>
        /// <param name="name">The name.</param>
        /// <param name="text">The text.</param>
        /// <param name="expectedOutput">The expected output.</param>
        /// created 14.07.2007 14:34 on HAHOYER-DELL by hh
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
            if(os.Data != expectedOutput)
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
    }
}