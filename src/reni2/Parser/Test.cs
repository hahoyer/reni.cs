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
using HWClassLibrary.Debug;
using HWClassLibrary.UnitTest;
using Reni.FeatureTest;
using Reni.FeatureTest.Helper;

namespace Reni.Parser
{
    [TestFixture]
    public sealed class ParserTest : CompilerTest
    {
        public override void Run() { }

        //[Test]
        public void SimpleFunction()
        {
            var syntaxPrototype = LikeSyntax.Expression(null, "f", LikeSyntax.Null);
            Parameters.Trace.Source = true;
            Parameters.ParseOnly = true;
            CreateFileAndRunCompiler("SimpleFunction", @"f()", expectedResult:c => syntaxPrototype.AssertLike(c.Syntax));
        }

        [Test]
        public void Add2Numbers()
        {
            var syntaxPrototype =
                (LikeSyntax.Number(2) + LikeSyntax.Number(4)).dump_print;
            Parameters.ParseOnly = true;
            CreateFileAndRunCompiler("Add2Numbers", @"(2+4) dump_print", expectedResult: c => syntaxPrototype.AssertLike(c.Syntax));
        }

        [Test]
        public void LineComment()
        {
            var syntaxPrototype =
                (LikeSyntax.Number(2) + LikeSyntax.Number(4)).dump_print;
            Parameters.ParseOnly = true;
            CreateFileAndRunCompiler("Add2Numbers", @"
(2+4) #ssssss
dump_print
", expectedResult: c => syntaxPrototype.AssertLike(c.Syntax));
        }

        [Test]
        public void BlockComment()
        {
            var syntaxPrototype =
                (LikeSyntax.Number(2) + LikeSyntax.Number(4)).dump_print;
            Parameters.ParseOnly = true;
            CreateFileAndRunCompiler("Add2Numbers", @"
(2+4) #(aa ssssss
aa)#dump_print
", expectedResult: c => syntaxPrototype.AssertLike(c.Syntax));
        }

        [Test]
        public void AlternativePrioTableConverter()
        {
            var syntaxPrototype = LikeSyntax.Struct(
                new[] {LikeSyntax.Number(3)},
                new Declaration[] {},
                new[] {0}
                );
            Parameters.ParseOnly = true;
            CreateFileAndRunCompiler("UseAlternativePrioTable", @"!converter: 3", expectedResult: c => syntaxPrototype.AssertLike(c.Syntax));
        }

        [Test]
        public void AlternativePrioTableConverterAndProperty()
        {
            var syntaxPrototype = LikeSyntax.Struct(
                new[] {LikeSyntax.Number(3), LikeSyntax.Number(4)},
                new Declaration[] {},
                new[] {0, 1}
                );
            Parameters.ParseOnly = true;
            CreateFileAndRunCompiler("UseAlternativePrioTable", @"!converter: 3; !converter: 4",
                                     expectedResult: c => syntaxPrototype.AssertLike(c.Syntax));
        }
    }
}