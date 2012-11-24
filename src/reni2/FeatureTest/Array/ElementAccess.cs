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

using System.Linq;
using System.Collections.Generic;
using System;
using HWClassLibrary.Debug;
using HWClassLibrary.UnitTest;

namespace Reni.FeatureTest.Array
{
    [TestFixture]
    [ArrayFromPieces]
    [TargetSet("((<<5) (0)) dump_print", "5")]
    public sealed class ElementAccessSimple : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [ElementAccessSimple]
    [TargetSet("((<<5<<3<<5<<1<<3) (3)) dump_print", "1")]
    public sealed class ElementAccess : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [ArrayFromPieces]
    [TargetSet("x: <<5<<3; x dump_print", "array(#(#align3#)# (bit)sequence(4),(5, 3))")]
    [LowPriority]
    public sealed class ArrayVariable : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [ElementAccessVariable]
    [ArrayVariable]
    [TargetSet("x: <<5<<3<<5<<1<<3; x(3) := 2; x dump_print", "array(#(#align3#)# (bit)sequence(4),(5, 3, 5, 2, 3))")]
    public sealed class ElementAccessVariableSetter : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [ElementAccess]
    [TargetSet("x: <<5<<3<<5<<1<<3; x(3) dump_print", "1")]
    public sealed class ElementAccessVariable : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }
}