using System;
using HWClassLibrary.Debug;
using NUnit.Framework;
using Reni.FeatureTest.Struct;

namespace Reni.FeatureTest.Function
{
    [TestFixture]
    public class FunctionWithNonLocal : CompilerTest
    {
        public override string Target
        {
            get
            {
                return @"
x: 100;
f: function arg+x;
f(2) dump_print;
";
            }
        }
        public override string Output { get { return "102"; } }
        public override System.Type[] DependsOn
        {
            get
            {
                return new[]
                {
                    typeof(InnerAccessTheOnlyOne), 
                    typeof(BitArrayOp),
                };
            }
        }


        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    public class PrimitiveRecursiveFunctionByteWithDump : CompilerTest
    {
        public override string Target { get { return @"i: 10; f: function i > 0 then (i := i - 1; i dump_print; f());f()"; } }
        public override string Output { get { return "9876543210"; } }
        public override System.Type[] DependsOn
        {
            get
            {
                return new[]
                {
                    typeof(InnerAccessTheOnlyOne), 
                    typeof(BitArrayOp),
                    typeof(ThenElse),
                    typeof(Assignment),
                };
            }
        }

        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }


    [TestFixture]
    public class PrimitiveRecursiveFunctionHuge : CompilerTest
    {
        public override string Target { get { return @"i: 400000; f: function i > 0 then (i := i - 1; f());f()"; } }
        public override string Output { get { return ""; } }
        public override System.Type[] DependsOn { get { return new[] { typeof(PrimitiveRecursiveFunctionSmall) }; } }

        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }

    /// <summary>
    /// Recursive function that will result in a stack overflow, except when compiled as a loop
    /// </summary>
    [TestFixture]
    public class PrimitiveRecursiveFunctionSmall : CompilerTest
    {
        public override string Target { get { return @"i: 400000 type(400); f: function i > 0 then (i := i - 1; f());f()"; } }
        public override string Output { get { return ""; } }
        public override System.Type[] DependsOn { get { return new[] { typeof(PrimitiveRecursiveFunctionByteWithDump) }; } }

        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    public class PrimitiveRecursiveFunctionWithDump : CompilerTest
    {
        public override string Target { get { return @"i: 400000 type(10); f: function i > 0 then (i := i - 1; i dump_print; f());f()"; } }
        public override string Output { get { return "9876543210"; } }
        public override System.Type[] DependsOn { get { return new[] { typeof(PrimitiveRecursiveFunctionByteWithDump) }; } }

        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }
    [TestFixture]
    public class RecursiveFunction : CompilerTest
    {
        public override string Target { get { return @"f: function arg = 1 then arg type(1) else arg * f(arg type((arg-1)enable_cut));f(4)dump_print"; } }
        public override string Output { get { return "24"; } }
        public override System.Type[] DependsOn
        {
            get
            {
                return new[]
                {
                    typeof(InnerAccessTheOnlyOne), 
                    typeof(BitArrayOp),
                    typeof(ThenElse),
                    typeof(DefaultOperations.ApplyTypeOperator),
                    typeof(DefaultOperations.ApplyCompareOperator),
                    typeof(DefaultOperations.ApplyTypeOperatorWithCut),
                };
            }
        }

        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }
    [TestFixture]
    public class FunctionWithRefArg : CompilerTest
    {
        public override string Target { get { return @"f: function arg;g: function f(arg);x:4; g(x)dump_print"; } }
        public override string Output { get { return "4"; } }

        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }
    [TestFixture]
    public class SimpleFunction : CompilerTest
    {
        public override string Target { get { return @"f: function arg+1;f(2) dump_print;"; } }
        public override string Output { get { return "3"; } }

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
                return @"
x: 100;
f1: function 
((
  y: 3;
  f: function arg*y+x;
  f(2)
) _A_T_ 2);

f1()dump_print;
";
            }
        }
        public override string Output { get { return "106"; } }

        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }
    [TestFixture]
    public class TwoFunctions1 : CompilerTest
    {
        public override string Target
        {
            get
            {
                return @"
f1: function 
((
  y: 3;
  f: function y;
  f(2)
) _A_T_ 2);

f1()dump_print;
";
            }
        }
        public override string Output { get { return "3"; } }
        public override void AssertValid(Compiler c)
        {
            var x = new ExpectedCompilationResult(c);
            Tracer.Assert(x.FunctionCount() == 2);
        }

        [Test, Category(Worked)]
        public override void Run() { BaseRun(); }
    }
}