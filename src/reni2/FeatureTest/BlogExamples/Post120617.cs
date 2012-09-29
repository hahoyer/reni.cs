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

using HWClassLibrary.UnitTest;

namespace Reni.FeatureTest.BlogExamples
{
    [TestFixture]
    [TargetSet("\"Hello world\" dump_print", "Hello world")]
    [TargetSet(@"Viersich: 4;
EinsDazu: /\ arg + 1 ;
Konstrukt: 
/\(
    Simpel: arg; 
    Pelsim: EinsDazu(arg); 
    Fun: /\ Simpel+ EinsDazu(arg)
);
lorum: Konstrukt(23);
ipsum: Konstrukt(8);
ipsum Pelsim := 15;
(Viersich, ipsum Simpel, ipsum Pelsim, ipsum Fun(7), lorum Simpel, lorum Fun(18)) dump_print"
        , "(4, 8, 15, 16, 23, 42)")]
    public sealed class Post120617 : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }
}