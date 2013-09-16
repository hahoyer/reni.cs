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
using Reni.FeatureTest.BitArrayOp;
using Reni.FeatureTest.Structure;
using Reni.FeatureTest.ThenElse;

namespace Reni.FeatureTest.Function
{
    [TestFixture]
    [InnerAccess]
    [Add2Numbers]
    [UseThen]
    [UseElse]
    [Assignments]
    [SimpleFunctionWithNonLocal]
    [RecursiveFunction]
    [NamedSimpleAssignment]
    [Target(@"i: 10; f: /\ i > 0 then (i := (i - 1)enable_cut; i dump_print; f());f()")]
    [Output("9876543210")]
    public sealed class PrimitiveRecursiveFunctionByteWithDump : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }
}