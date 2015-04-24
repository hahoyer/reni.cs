using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Parser;
using hw.UnitTest;

namespace Reni.ParserTest
{
    [UnitTest]
    sealed class PrioTableTest : DependantAttribute
    {
        [UnitTest]
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