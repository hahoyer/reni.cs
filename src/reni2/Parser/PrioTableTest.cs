#region Copyright (C) 2013

//     Project Reni2
//     Copyright (C) 2012 - 2013 Harald Hoyer
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
using hw.PrioParser;
using hw.UnitTest;

namespace Reni.Parser
{
    [TestFixture]
    sealed class PrioTableTest : DependantAttribute
    {
        [Test]
        public void FromText()
        {
            var text = @"
Right **
Left * /
Left + -
";
            var prioTable = PrioTable.FromText(text);
            var dump = prioTable.ToString();
            Tracer.FlaggedLine("\n" + dump);
            var expected = @"         00000000
         01234567
(bot) 00 +++++++-
(any) 01 +-+++++-
   ** 02 +-+++++-
    * 03 +----++-
    / 04 +----++-
    + 05 +-------
    - 06 +-------
(eot) 07 =-------
";
            Tracer.Assert(dump == expected.Replace("\r\n", "\n"));
        }
    }
}