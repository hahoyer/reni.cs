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

using System.Linq;
using System.Collections.Generic;
using System;
using HWClassLibrary.Debug;
using HWClassLibrary.UnitTest;

namespace Reni.FeatureTest.Structure
{
    [TestFixture]
    [TargetSet(@"a: (a: 11, b:222, c:4, d: 2722); a b dump_print", "222")]
    [TargetSet(@"a: (a: 11, b:222, c:4, d: 2722); a c dump_print", "4")]
    [InnerAccess]
    public sealed class AccessMember : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }
}