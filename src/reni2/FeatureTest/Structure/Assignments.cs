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
using hw.Debug;
using hw.UnitTest;

namespace Reni.FeatureTest.Structure
{
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
    public sealed class Assignments : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }
}