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
using Reni.FeatureTest.Function;

namespace Reni.FeatureTest.Structure
{
    [TestFixture]
    [InnerAccess]
    [Function.Function]
    [SimpleFunctionWithNonLocal]
    [FunctionAssignment]
    public sealed class PropertyVariable : CompilerTest
    {
        protected override string Target => @"x: /!\11; x dump_print";
        protected override string Output => "11";

        [Test]
        public override void Run() => BaseRun();
    }
}