#region Copyright (C) 2012

//     Project Reni2
//     Copyright (C) 2011 - 2012 Harald Hoyer
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//     
//     Comments, bugs and suggestions to hahoyer at yahoo.de

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.UnitTest;
using Reni.FeatureTest.BitArrayOp;
using Reni.FeatureTest.DefaultOperations;
using Reni.FeatureTest.Struct;
using Reni.FeatureTest.ThenElse;
using Reni.FeatureTest.TypeType;

namespace Reni.FeatureTest.Function
{
    [LowPriority]
    [TestFixture]
    [TargetSet(@"a:(x: 100;f: arg+x/\); a f(1) function_instance ^ dump_print;", @"(100, (arg)+(x)/\)")]
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
    [SimpleFunction]
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
    [PropertyVariable]
    [Target(@"f: (value: arg, x: value/!\)/\;f(2) x dump_print")]
    [Output("2")]
    public sealed class ObjectProperty : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [ObjectProperty]
    [Target(@"f: (value: arg, x: value/\)/\;f(2) x(100) dump_print")]
    [Output("2")]
    public sealed class ObjectFunction1 : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [ObjectProperty]
    [Target(@"f: (value: arg, x: arg/\)/\;f(2) x(100) dump_print")]
    [Output("100")]
    public sealed class ObjectFunction2 : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [ObjectFunction1]
    [ObjectFunction2]
    [Target(@"f: (value: arg, x: arg+value/\)/\;f(2) x(100) dump_print")]
    [Output("102")]
    public sealed class ObjectFunction : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [InnerAccess]
    [Add2Numbers]
    [UseThen]
    [UseElse]
    [Assignments]
    [SimpleFunctionWithNonLocal]
    [RecursiveFunction]
    [NamedSimpleAssignment]
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
    //[LowPriority]
    public sealed class PrimitiveRecursiveFunctionHuge : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    /// <summary>
    ///     Recursive function that will result in a stack overflow, except when compiled as a loop
    /// </summary>
    [TestFixture]
    [Target(@"i: 400000 type instance(400); f: i > 0 then (i := (i - 1)enable_cut; f())/\;f()")]
    [Output("")]
    [PrimitiveRecursiveFunctionByteWithDump]
    public sealed class PrimitiveRecursiveFunctionSmall : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [TargetSet(@"i: 400000 type instance(10); f: i > 0 then (i := (i - 1)enable_cut; i dump_print; f())/\;f()", "9876543210")]
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
    [TwoFunctions1]
    [FunctionWithRefArg]
    [Target(@"f: {1000 type instance({arg = 1 then 1 else (arg * f[arg type instance((arg-1)enable_cut)])}enable_cut)}/\;f(4)dump_print")]
    [Output("24")]
    public sealed class RecursiveFunction : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [Target(@"f: arg/\;g: f(arg)/\;x:4; g(x)dump_print")]
    [Output("4")]
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
    [SimpleFunctionWithNonLocal]
    [TwoFunctions1]
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
    [SimpleFunctionWithNonLocal]
    [ObjectFunction]
    public sealed class TwoFunctions1 : CompilerTest
    {
        protected override void AssertValid(Compiler c)
        {
            var x = new ExpectedCompilationResult(c);
            //Tracer.Assert(x.FunctionCount() == 2);
        }

        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [SimpleFunctionWithNonLocal]
    [TargetSet(@"f: arg/\(arg + new_value)dump_print;f(100) := 2;", "102")]
    public sealed class FunctionAssignment : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }
}