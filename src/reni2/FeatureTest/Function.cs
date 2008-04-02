using HWClassLibrary.Debug;
using NUnit.Framework;

namespace Reni.FeatureTest
{
    /// <summary>
    /// Functions
    /// </summary>
    [TestFixture]
    public class Function : CompilerTest
    {
        #region Setup/Teardown

        /// <summary>
        /// Starts this instance.
        /// </summary>
        /// created 10.01.2007 04:32
        [SetUp]
        public new void Start()
        {
            base.Start();
        }

        #endregion

        /// <summary>
        /// Two functions.
        /// </summary>
        /// created 08.10.2006 16:33
        [Test, Category(Worked)]   
        public void FunctionWithNonLocal()
        {
            Parameters.Trace.CodeTree = true;
            RunCompiler("FunctionWithNonLocal",
                @"
x: 100;
f: function arg+x;
f(2) dump_print;
"
                , "102"
                );
        }

        /// <summary>
        /// Recursive function that will result in a stack overflow, except when compiled as a loop
        /// </summary>
        [Test, Category(Worked)]
        public void PrimitiveRecursiveFunctionByteWithDump()
        {
            RunCompiler
                (
                "PrimitiveRecursiveFunctionByteWithDump",
                @"i: 10; f: function i > 0 then (i := i - 1; i dump_print; f());f()",
                "9876543210"
                );
        }

        /// <summary>
        /// Recursive function that will result in a stack overflow, except when compiled as a loop
        /// </summary>
        [Test, Category(Worked)]
        public void PrimitiveRecursiveFunctionHuge()
        {
            RunCompiler
                (
                "PrimitiveRecursiveFunctionHuge",
                @"i: 400000; f: function i > 0 then (i := i - 1; f());f()",
                ""
                );
        }

        /// <summary>
        /// Recursive function that will result in a stack overflow, except when compiled as a loop
        /// </summary>
        [Test, Category(Worked)]
        public void PrimitiveRecursiveFunctionSmall()
        {
            RunCompiler
                (
                "PrimitiveRecursiveFunctionSmall",
                @"i: 400000 type(400); f: function i > 0 then (i := i - 1; f());f()",
                ""
                );
        }

        /// <summary>
        /// Recursive function that will result in a stack overflow, except when compiled as a loop
        /// </summary>
        [Test, Category(Worked)]
        public void PrimitiveRecursiveFunctionWithDump()
        {
            RunCompiler
                (
                "PrimitiveRecursiveFunctionSmall",
                @"i: 400000 type(10); f: function i > 0 then (i := i - 1; i dump_print; f());f()",
                "9876543210"
                );
        }

        /// <summary>
        /// Recursive function.
        /// </summary>
        /// created 05.01.2007 02:13
        [Test, Category(Worked)]
        public void RecursiveFunction()
        {
            RunCompiler
                (
                "RecursiveFunction",
                @"f: function arg = 1 then arg type(1) else arg * f(arg type((arg-1)enable_cut));f(4)dump_print",
                "24"
                );
        }

        /// <summary>
        /// Recursive function.
        /// </summary>
        /// created 05.01.2007 02:13
        [Test, Category(UnderConstruction)]
        public void FunctionWithRefArg()
        {
            RunCompiler
                (
                "FunctionWithRefArg",
                @"f: function arg;g: function f(arg);x:4; g(x)dump_print",
                "4"
                );
        }

        /// <summary>
        /// Simples the function.
        /// </summary>
        /// created 08.10.2006 16:33
        [Test, Category(Worked)]   
        public void SimpleFunction()
        {
            RunCompiler("SimpleFunction",
                @"
f: function arg+1;
f(2) dump_print;
"
                , "3"
                );
        }

        /// <summary>
        /// Two functions.
        /// </summary>
        /// created 08.10.2006 16:33
        [Test, Category(Worked)]   
        public void TwoFunctions()
        {
            RunCompiler("TwoFunctions",
                @"
x: 100;
f1: function 
((
  y: 3;
  f: function arg*y+x;
  f(2)
) _A_T_ 2);

f1()dump_print;
"
                , "106"
                );
        }

        /// <summary>
        /// Two functions.
        /// </summary>
        /// created 08.10.2006 16:33
        [Test, Category(Worked)]   
        public void TwoFunctions1()
        {
            RunCompiler
                (
                "TwoFunctions1",
                @"
f1: function 
((
  y: 3;
  f: function y;
  f(2)
) _A_T_ 2);

f1()dump_print;
",
                delegate(Compiler c)
                {
                    var x = new ExpectedCompilationResult(c);
                    Tracer.Assert(x.FunctionCount() == 2);
                },
                "3"
                );
        }
    }
}