using System;
using HWClassLibrary.Debug;
using NUnit.Framework;
using Reni.FeatureTest.BitArrayOp;
using Reni.FeatureTest.DefaultOperations;
using Reni.FeatureTest.Struct;
using Reni.FeatureTest.ThenElse;

namespace Reni.FeatureTest.Function
{
    [TestFixture]
    [Target(@"a:(x: 100;f: arg+x/\);g: a f; g \|/ dump_print;")]
    [Output("102")]
    [InnerAccessTheOnlyOne, Add2Numbers]
    public class FunctionVariable : CompilerTest
    {
        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [Target(@"x: 100;f: arg+x/\;f(200) dump_print;")]
    [Output("102")]
    [InnerAccessTheOnlyOne, Add2Numbers]
    public class FunctionWithNonLocal : CompilerTest
    {
        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }

    [TestFixture, InnerAccessTheOnlyOne, Add2Numbers, UseThen, UseElse, Assignment, SimpleFunction, RecursiveFunction]
    [Target(@"i: 10; f: i > 0 then (i := (i - 1)enable_cut; i dump_print; f())/\;f()")]
    [Output("9876543210")]
    public class PrimitiveRecursiveFunctionByteWithDump : CompilerTest
    {
        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [Target(@"i: 400000; f: i > 0 then (i := (i - 1)enable_cut; f())/\;f()")]
    [Output("")]
    [PrimitiveRecursiveFunctionSmall]
    public class PrimitiveRecursiveFunctionHuge : CompilerTest
    {
        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }

    /// <summary>
    /// Recursive function that will result in a stack overflow, except when compiled as a loop
    /// </summary>
    [TestFixture]
    [Target(@"i: 400000 type(400); f: i > 0 then (i := (i - 1)enable_cut; f())/\;f()")]
    [Output("")]
    [PrimitiveRecursiveFunctionByteWithDump]
    public class PrimitiveRecursiveFunctionSmall : CompilerTest
    {
        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [Target(@"i: 400000 type(10); f: i > 0 then (i := (i - 1)enable_cut; i dump_print; f())/\;f()")]
    [Output("9876543210")]
    [PrimitiveRecursiveFunctionByteWithDump, UseThen, UseElse]
    public class PrimitiveRecursiveFunctionWithDump : CompilerTest
    {
        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }

    [TestFixture, InnerAccessTheOnlyOne, Add2Numbers, UseThen, UseElse, ApplyTypeOperator, Equal, ApplyTypeOperatorWithCut, SimpleFunction]
    [Target(@"f: {1000 type({arg = 1 then 1 else (arg * f(arg type((arg-1)enable_cut))}enable_cut)}/\;f(4)dump_print")]
    [Output("24")]
    public class RecursiveFunction : CompilerTest
    {
        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }

    [TestFixture, Target(@"f: arg/\;g: f(arg)/\;x:4; g(x)dump_print"), Output("4"), UseThen, UseElse, SimpleFunction]
    public class FunctionWithRefArg : CompilerTest
    {
        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }

    [TestFixture,
    Target(@"f: arg+1/\;f(2) dump_print;"), Output("3"), 
    InnerAccessTheOnlyOne, Add2Numbers] 
    public class SimpleFunction : CompilerTest
    {
        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    public class TwoFunctions : CompilerTest
    {
        public override string Target
        {
            get
            {
                return
                    @"
x: 100;
f1: ((
  y: 3;
  f: arg*y+x/\;
  f(2)
) _A_T_ 2)/\;

f1()dump_print;
";
            }
        }

        protected override string Output { get { return "106"; } }

        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }

    [TestFixture, Target(@"
f1: ((
  y: 3;
  f: y/\;
  f(2)
) _A_T_ 2)/\;

f1()dump_print;
"), Output("3")]
    public class TwoFunctions1 : CompilerTest
    {
        protected override void AssertValid(Compiler c)
        {
            var x = new ExpectedCompilationResult(c);
            //Tracer.Assert(x.FunctionCount() == 2);
        }

        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }
}