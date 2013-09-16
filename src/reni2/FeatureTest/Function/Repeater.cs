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
using Reni.FeatureTest.DefaultOperations;
using Reni.FeatureTest.Structure;
using Reni.FeatureTest.ThenElse;
using Reni.FeatureTest.TypeType;

namespace Reni.FeatureTest.Function
{
    [TestFixture]
    [Target(@"
repeat: /\arg() while then repeat(arg);

count: 10,
index: count type instance(0),
repeat
(/\(
    while: index < count, 
    while then
    (
        index dump_print, 
        ' ' dump_print, 
        index := (index + 1)enable_cut
    )
))
")]
    [Output("0 1 2 3 4 5 6 7 8 9 ")]
    [Equal]
    [Assignments]
    [TypeOperator]
    [BitArrayOp.BitArrayOp]
    [UseThen]
    [FunctionOfFunction]
    public sealed class SimpleRepeater: CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }

    [TestFixture]
    [Target(@"
repeat: /\arg while() then(arg body(), repeat(arg));

count: 10,
index: count type instance(0),
repeat
(
    while: /\ index < count, 
    body: /\
    (
        index dump_print, 
        ' ' dump_print, 
        index := (index + 1)enable_cut
    )
)
")]
    [Output("0 1 2 3 4 5 6 7 8 9 ")]
    [SimpleRepeater]
    public sealed class Repeater : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }
}