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

using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.UnitTest;
using Reni.FeatureTest.Array;
using Reni.FeatureTest.BitArrayOp;

namespace Reni.FeatureTest.Text
{
    [TestFixture]
    [TargetSet(@"'Hallo' dump_print", "Hallo")]
    public sealed class Hallo : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [TargetSet("'Hal''lo' dump_print", "Hal'lo")]
    [Hallo]
    public sealed class HalloApo : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [TargetSet("\"Hal''lo\" dump_print", "Hal''lo")]
    [HalloApo]
    public sealed class HalloApoApo : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [TargetSet("\"Hal'\"\"'lo\" dump_print", "Hal'\"'lo")]
    [HalloApoApo]
    public sealed class HalloApoQuoApo : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [TargetSet("('Hallo' << ' Welt!') dump_print", "Hallo Welt!")]
    [Hallo]
    [CombineArraysFromPieces]
    public sealed class HalloWelt : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [Hallo]
    [TargetSet("text_item(108) dump_print", "l")]
    public sealed class ConvertFromNumber : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [ConvertFromNumber]
    [TargetSet("(<< text_item(108)<< text_item(109))text_items dump_print", "lm")]
    [ArrayFromPieces]
    [Hallo]
    public sealed class ConvertFromNumbers : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [Hallo]
    [Number]
    [ConvertFromNumber]
    [TargetSet("('80' to_number_of_base 16) dump_print", "128")]
    public sealed class ConvertHexadecimal : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }
}