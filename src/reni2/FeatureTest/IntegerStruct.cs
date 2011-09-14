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
using Reni.FeatureTest.DefaultOperations;
using Reni.FeatureTest.Function;
using Reni.FeatureTest.Struct;

namespace Reni.FeatureTest.Integer
{
    /// <summary>
    ///     Structure, that is all between brackets
    /// </summary>
    public class IntegerStruct : CompilerTest
    {
        private static string Definition()
        {
            return
                @"
Integer8: 
{
    _data: 127 type (arg enable_cut);

    create   : (Integer8(arg))/\;
    dump_print: (_data dump_print)/\ auto_call;
    +        :  create(_data + create(arg) _data)/\;
    clone: create(_data)/\  auto_call;
    enable_cut: _data enable_cut /\  auto_call;
    !converter: _data /\ ;
}/\
";
        }

        public override void Run() { }
        protected override string Target { get { return Definition() + "; " + InstanceCode + " dump_print"; } }
        protected virtual string InstanceCode { get { return GetStringAttribute<InstanceCodeAttribute>(); } }
    }

    [TestFixture]
    [Output("3")]
    [InstanceCode("(Integer8(1)+Integer8(2))")]
    [IntegerPlusNumber]
    public sealed class IntegerPlusInteger : IntegerStruct
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [Output("3")]
    [InstanceCode("(Integer8(1)+2)")]
    [Create]
    [ObjectFunction]
    [TwoFunctions1]
    public sealed class IntegerPlusNumber : IntegerStruct
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [Output("23")]
    [InstanceCode("Integer8(23) clone")]
    [Create]
    [TwoFunctions1]
    [RecursiveFunction]
    public sealed class Clone : IntegerStruct
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [Output("23")]
    [InstanceCode("Integer8(0) create(23)")]
    [Integer1]
    [Integer2]
    [Integer127]
    public sealed class Create : IntegerStruct
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [PropertyVariable]
    [ApplyTypeOperatorWithCut]
    [SimpleFunctionWithNonLocal]
    [ObjectProperty]
    [Output("1")]
    [InstanceCode("Integer8(1)")]
    public sealed class Integer1 : IntegerStruct
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [Output("2")]
    [Integer1]
    [InstanceCode("Integer8(2)")]
    public sealed class Integer2 : IntegerStruct
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [Integer2]
    [Output("127")]
    [InstanceCode("Integer8(127)")]
    public sealed class Integer127 : IntegerStruct
    {
        [Test]
        public override void Run() { BaseRun(); }
    }
}