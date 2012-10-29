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

using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.UnitTest;
using Reni.FeatureTest.Array;
using Reni.FeatureTest.Text;
using Reni.FeatureTest.TypeType;

namespace Reni.FeatureTest.Reference
{
    [TestFixture]
    [TypeOperator]
    [Hallo01234]
    [ElementAccessVariable]
    [Target(@"
x: 'Hallo World';
y: x(0) reference;
y type dump_print;
")]
    [Output("((bit)sequence(8) #(# text_item #)#)reference(1)")]
    public sealed class TypeOfConversionFromPointer : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }
   
    [TestFixture]
    [TypeOfConversionFromPointer]
    [Target(@"
x: 'Hallo World';
y: x(0) reference;
y(0) dump_print;
")]
    [Output("H")]
    public sealed class ConversionFromPointer : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }
}