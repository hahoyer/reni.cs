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

using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.UnitTest;
using Reni.FeatureTest.Structure;

namespace Reni.FeatureTest.ThenElse
{
    [TestFixture]
    [Target(@"x: 1=1 then 1 else 100;x dump_print;")]
    [Output("1")]
    [InnerAccess]
    [SomeVariables]
    public sealed class UseThen : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [Target(@"x: 1=0 then 1 else 100;x dump_print;")]
    [Output("100")]
    [InnerAccess]
    [SomeVariables]
    public sealed class UseElse : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }
}

namespace Reni.FeatureTest.Validation
{
}

