#region Copyright (C) 2012

//     Project Reni2
//     Copyright (C) 2012 - 2012 Harald Hoyer
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

using System.Collections.Generic;
using HWClassLibrary.UnitTest;

namespace Reni.FeatureTest.TypeType
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
    [TypeOperator]
    [TargetSet("(0 type * 3) sequence dump_print", "(#(#align3#)# (bit)sequence(1))sequence(3)")]
    public sealed class SequenceOfType : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }
}