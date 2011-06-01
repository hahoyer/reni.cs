using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.UnitTest;
using Reni.FeatureTest.BitArrayOp;
using Reni.FeatureTest.DefaultOperations;
using Reni.FeatureTest.Struct;
using Reni.FeatureTest.ThenElse;

namespace Reni.FeatureTest.Function
{
    [TestFixture]
    [TargetSet(@"a:(x: 100;f: arg+x/\);g: a f; g \|/ dump_print;", @"(100, (arg)+(x)/\)")]
    [SomeVariables]
    [Add2Numbers]
    [AccessMember]
    [FunctionWithNonLocal]
    [Function]
    [TwoFunctions]
    [FunctionWithRefArg]
    public sealed class FunctionVariable : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [Target(@"f: arg/\;f(2) dump_print;")]
    [Output("2")]
    [InnerAccess]
    [SomeVariables]
    public sealed class Function : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [Target(@"f: 100/\;f() dump_print;")]
    [Output("100")]
    [Function]
    public sealed class ConstantFunction : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [Target(@"x: 100; f: x/\;f() dump_print;")]
    [Output("100")]
    [InnerAccess]
    [SomeVariables]
    [ConstantFunction]
    public sealed class SimpleFunctionWithNonLocal : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [Target(@"x: 100;f: arg+x/\;f(2) dump_print;")]
    [Output("102")]
    [InnerAccess]
    [SomeVariables]
    [Add2Numbers]
    [SimpleFunctionWithNonLocal]
    public sealed class FunctionWithNonLocal : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [FunctionWithNonLocal]
    [Target(@"f: (value: arg, + : value + arg/\)/\;(f(2)+3) dump_print")]
    [Output("5")]
    public sealed class ObjectFunction : CompilerTest
    {
        [Test, IsUnderConstruction]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [InnerAccess]
    [Add2Numbers]
    [UseThen]
    [UseElse]
    [Assignment]
    [SimpleFunction]
    [RecursiveFunction]
    [Target(@"i: 10; f: i > 0 then (i := (i - 1)enable_cut; i dump_print; f())/\;f()")]
    [Output("9876543210")]
    public sealed class PrimitiveRecursiveFunctionByteWithDump : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [Target(@"i: 400000; f: i > 0 then (i := (i - 1)enable_cut; f())/\;f()")]
    [Output("")]
    [PrimitiveRecursiveFunctionSmall]
    public sealed class PrimitiveRecursiveFunctionHuge : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    /// <summary>
    ///     Recursive function that will result in a stack overflow, except when compiled as a loop
    /// </summary>
    [TestFixture]
    [Target(@"i: 400000 type(400); f: i > 0 then (i := (i - 1)enable_cut; f())/\;f()")]
    [Output("")]
    [PrimitiveRecursiveFunctionByteWithDump]
    public sealed class PrimitiveRecursiveFunctionSmall : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [TargetSet(@"i: 400000 type(10); f: i > 0 then (i := (i - 1)enable_cut; i dump_print; f())/\;f()", "9876543210")]
    [PrimitiveRecursiveFunctionByteWithDump]
    [UseThen]
    [UseElse]
    public sealed class PrimitiveRecursiveFunctionWithDump : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [InnerAccess]
    [Add2Numbers]
    [UseThen]
    [UseElse]
    [ApplyTypeOperator]
    [Equal]
    [ApplyTypeOperatorWithCut]
    [SimpleFunction]
    [Target(@"f: {1000 type({arg = 1 then 1 else (arg * f[arg type((arg-1)enable_cut)])}enable_cut)}/\;f(4)dump_print")]
    [Output("24")]
    public sealed class RecursiveFunction : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [Target(@"f: arg/\;g: f(arg)/\;x:4; g(x)dump_print")]
    [Output("4")]
    [UseThen]
    [UseElse]
    [SimpleFunction]
    public sealed class FunctionWithRefArg : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [TargetSet(@"f: arg+1/\;f(2) dump_print;", "3")]
    [InnerAccess]
    [Add2Numbers]
    [Function]
    public sealed class SimpleFunction : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [SimpleFunction]
    public sealed class TwoFunctions : CompilerTest
    {
        protected override string Target
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

        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [Target(@"
f1: ((
  y: 3;
  f: y/\;
  f(2)
) _A_T_ 2)/\;

f1()dump_print;
")]
    [Output("3")]
    [TwoFunctions]
    public sealed class TwoFunctions1 : CompilerTest
    {
        protected override void AssertValid(Compiler c)
        {
#pragma warning disable 168
            var x = new ExpectedCompilationResult(c);
#pragma warning restore 168
            //Tracer.Assert(x.FunctionCount() == 2);
        }

        [Test]
        public override void Run() { BaseRun(); }
    }
}