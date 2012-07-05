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
using Reni.FeatureTest.Struct;

namespace Reni.FeatureTest.DefaultOperations
{
    [TestFixture]
    public sealed class TypeOperator : CompilerTest
    {
        protected override string Target { get { return @"31 type dump_print"; } }
        protected override string Output { get { return "(bit)sequence(6)"; } }

        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    public sealed class TypeOperatorWithVariable : CompilerTest
    {
        protected override string Target { get { return @"x: 0; x type dump_print"; } }
        protected override string Output { get { return "(bit)sequence(1)"; } }
        protected override IEnumerable<System.Type> DependsOn { get { return new[] {typeof(SomeVariables), typeof(TypeOperator)}; } }

        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    public sealed class ApplyTypeOperator : CompilerTest
    {
        protected override string Target { get { return @"(31 type instance (28))dump_print"; } }
        protected override string Output { get { return "28"; } }
        protected override IEnumerable<System.Type> DependsOn { get { return new[] {typeof(TypeOperator)}; } }

        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [ApplyTypeOperator]
    public sealed class ApplyTypeOperatorWithCut : CompilerTest
    {
        protected override string Target { get { return @"(31 type (100 enable_cut))dump_print"; } }
        protected override string Output { get { return "-28"; } }
        protected override IEnumerable<System.Type> DependsOn { get { return new[] {typeof(ApplyTypeOperator)}; } }

        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    public abstract class ApplyCompareOperator : CompilerTest
    {
        protected override string Target { get { return "(1" + Operator + "100)dump_print"; } }
        protected abstract string Operator { get; }
        protected abstract bool Result { get; }
        protected override string Output { get { return Result ? "-1" : "0"; } }
        protected override IEnumerable<System.Type> DependsOn { get { return new[] {typeof(Add2Numbers)}; } }

        [Test]
        public override void Run() { BaseRun(); }
    }

    public sealed class Equal : ApplyCompareOperator
    {
        protected override string Operator { get { return "="; } }
        protected override bool Result { get { return false; } }
    }

    public sealed class NotEqual : ApplyCompareOperator
    {
        protected override string Operator { get { return "<>"; } }
        protected override bool Result { get { return true; } }
    }

    public sealed class GreaterThan : ApplyCompareOperator
    {
        protected override string Operator { get { return ">"; } }
        protected override bool Result { get { return false; } }
    }

    public sealed class LessThan : ApplyCompareOperator
    {
        protected override string Operator { get { return "<"; } }
        protected override bool Result { get { return true; } }
    }

    public sealed class LessOrEqual : ApplyCompareOperator
    {
        protected override string Operator { get { return "<="; } }
        protected override bool Result { get { return true; } }
    }

    public sealed class GreaterOrEqual : ApplyCompareOperator
    {
        protected override string Operator { get { return ">="; } }
        protected override bool Result { get { return false; } }
    }
}