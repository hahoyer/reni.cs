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

using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.UnitTest;

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
    public sealed class HalloApo : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [TargetSet("\"Hal''lo\" dump_print", "Hal''lo")]
    public sealed class HalloApoApo : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [TargetSet("\"Hal'\"\"'lo\" dump_print", "Hal'\"'lo")]
    public sealed class HalloApoQuoApo : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [Hallo]
    [TargetSet("127 type(108) text_item dump_print", "l")]
    public sealed class ConvertFromNumber : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [ConvertFromNumber]
    [TargetSet("sequence (<< 108 text_item << 109 text_item) dump_print", "lm")]
    public sealed class ConvertFromNumbers : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }
}