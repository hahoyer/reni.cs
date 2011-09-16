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
using Reni.FeatureTest.Array;
using Reni.FeatureTest.Function;
using Reni.FeatureTest.Integer;

namespace Reni.FeatureTest
{
    public class TextStruct : CompilerTest
    {
        static string Definition()
        {
            return
                @"
!system:
{
    MaxNumber8: '7f' to_number_of_base(16) /!\ ;
    MaxNumber16: '7fff' to_number_of_base(16) /!\ ;
    MaxNumber32: '7fffffff' to_number_of_base(16) /!\ ;
    MaxNumber64: '7fffffffffffffff' to_number_of_base(16) /!\ ;

    TextItemType: text_item(MaxNumber8) type /!\ ;
}/!\ ;

Text: 
{
    data: (system TextItemType * system MaxNumber32) reference (args);
    _length: system MaxNumber32 type (arg type / system TextItemType);
    AfterCopy: data:= system NewMemory(system TextItemType,system MaxNumber32,data at arg /\)/\;
    AfterCopy()
}/\
";
        }

        public override void Run() { }
        protected override string Target { get { return Definition() + "; " + InstanceCode + " dump_print"; } }
        protected virtual string InstanceCode { get { return GetStringAttribute<InstanceCodeAttribute>(); } }
    }

    [TestFixture]
    [Output("a")]
    [InstanceCode("Text('a')")]
    [Integer1]
    [TwoFunctions]
    [FromTypeAndFunction]
    [PrimitiveRecursiveFunctionByteWithDump]
    public sealed class Text1 : TextStruct
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [Output("Hallo")]
    [InstanceCode("(Text('H') << 'allo'")]
    [Text1]
    public sealed class TextConcat : TextStruct
    {
        [Test]
        public override void Run() { BaseRun(); }
    }
}