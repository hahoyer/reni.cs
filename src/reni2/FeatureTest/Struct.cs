//     Compiler for programming language "Reni"
//     Copyright (C) 2011 Harald Hoyer
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

using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.UnitTest;
using Reni.FeatureTest.BitArrayOp;
using Reni.FeatureTest.DefaultOperations;

namespace Reni.FeatureTest.Struct
{
    [TestFixture]
    [TargetSet(@"((0, 1) _A_T_ 0) dump_print;", "0")]
    [TargetSet(@"((0, 1, ) _A_T_ 0) dump_print;", "0")]
    public sealed class AccessSimple : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [TargetSet(@"((0, 1) _A_T_ 1) dump_print;", "1")]
    public sealed class AccessSimple1 : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [AccessSimple]
    [InnerAccess]
    [AccessAndAdd]
    [TargetSet(@"((one: 1) one) dump_print;", "1")]
    [TargetSet(@"((one: 1,) one) dump_print;", "1")]
    [TargetSet(@"((0,one: 1) one) dump_print;", "1")]
    [TargetSet(@"((0,one: 1,) one) dump_print;", "1")]
    public sealed class StrangeStructs : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [AccessSimple]
    [AccessSimple1]
    [Target(@"
((0;1;2;300;) _A_T_ 0) dump_print;
((0;1;2;300;) _A_T_ 1) dump_print;
((0;1;2;300;) _A_T_ 2) dump_print;
((0;1;2;300;) _A_T_ 3) dump_print;
")]
    [Output("012300")]
    public sealed class Access : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [AccessSimple]
    [Target(@"(1, 2, 3, 4, 5, 6) dump_print")]
    [Output("(1, 2, 3, 4, 5, 6)")]
    public sealed class DumpPrint : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [InnerAccess]
    [Function.Function]
    public sealed class PropertyVariable : CompilerTest
    {
        protected override string Target { get { return @"! property x: 11/\; x dump_print"; } }
        protected override string Output { get { return "11"; } }

        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [InnerAccess]
    [TargetSet(@"(10, (^ _A_T_ 0) := 4) dump_print", "(4, )")]
    public sealed class SimpleAssignment : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [SimpleAssignment]
    [TargetSet(@"(x: 10, x := 4) dump_print", "(4, )")]
    public sealed class NamedSimpleAssignment : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [TargetSet(@"(10,20,30, (^ _A_T_ 1) := 4) dump_print", "(10, 4, 30, )")]
    [SimpleAssignment]
    public sealed class SimpleAssignment1 : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [TargetSet(@"(10,20, (^ _A_T_ 0) := 4) dump_print", "(4, 20, )")]
    [TargetSet(@"(10,20,30, (^ _A_T_ 0) := 4) dump_print", "(4, 20, 30, )")]
    [TargetSet(@"(10,20, (^ _A_T_ 1) := 4) dump_print", "(10, 4, )")]
    [TargetSet(@"(10,20,30, (^ _A_T_ 2) := 4) dump_print", "(10, 20, 4, )")]
    [TargetSet(@"(1000,20,30, (^ _A_T_ 0) := 4) dump_print", "(4, 20, 30, )")]
    [TargetSet(@"(1000,20,30, (^ _A_T_ 1) := 4) dump_print", "(1000, 4, 30, )")]
    [TargetSet(@"(10,2000,30, (^ _A_T_ 0) := 4) dump_print", "(4, 2000, 30, )")]
    [TargetSet(@"(10,2000,30, (^ _A_T_ 1) := 4) dump_print", "(10, 4, 30, )")]
    [TargetSet(@"(10,2000,30, (^ _A_T_ 2) := 4) dump_print", "(10, 2000, 4, )")]
    [SimpleAssignment]
    [SimpleAssignment1]
    public sealed class Assignment : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [TargetSet(@"(3, (^ _A_T_ 0) := 5 enable_cut) dump_print", "(-3, )")]
    [Assignment]
    [ApplyTypeOperatorWithCut]
    public sealed class AssignmentWithCut : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [AccessAndAdd]
    [TargetSet(" 1; 4;2050; (^ _A_T_ 0) + (^ _A_T_ 1) + (^ _A_T_ 2);(^ _A_T_ 3) dump_print;", "2055")]
    public sealed class AccessAndAddComplex : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [InnerAccess]
    [Add2Numbers]
    [TargetSet("5, (^ _A_T_ 0 + ^ _A_T_ 0)dump_print", "10")]
    public sealed class AccessAndAdd : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [Number]
    [TargetSet("5, (^ _A_T_ 0) dump_print, 66", "5")]
    [TargetSet("5,6, (^ _A_T_ 0) dump_print, 66", "5")]
    [TargetSet("5,6, (^ _A_T_ 1) dump_print, 66", "6")]
    public sealed class InnerAccess : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [TargetSet(@"
x1: 1;
x2: 4;
x3: 2050;
x4: x1 + x2 + x3;
x4 dump_print;
", "2055")]
    [AccessAndAddComplex]
    public sealed class SomeVariables : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [TargetSet(@"a: (a: 11, b:222, c:4, d: 2722); a b dump_print", "222")]
    [TargetSet(@"a: (a: 11, b:222, c:4, d: 2722); a c dump_print", "4")]
    [InnerAccess]
    public sealed class AccessMember : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }
}