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
using Reni.FeatureTest.Integer;

namespace Reni.FeatureTest
{
    public class StringStruct : CompilerTest
    {
        private static string Definition()
        {
            return
                @"
Character:
{
    _data: ' ' type element (arg);
}/\
;
String: 
{
}/\
";
        }

        public override void Run() { }
        protected override string Target { get { return Definition() + "; " + InstanceCode + " dump_print"; } }
        protected virtual string InstanceCode { get { return GetStringAttribute<InstanceCodeAttribute>(); } }
    }

    [Output("3")]
    [InstanceCode("(Character('a')dump_print")]
    [Integer1]
    public sealed class Character1: StringStruct
    {
        [Test]
        public override void Run() { BaseRun(); }
    }
}