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

namespace Reni.FeatureTest.Structure
{
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
}