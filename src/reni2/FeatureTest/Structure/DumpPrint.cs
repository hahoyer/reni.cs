using System.Linq;
using System.Collections.Generic;
using System;
using hw.UnitTest;
using Reni.FeatureTest.Helper;
using Reni.Parser;

namespace Reni.FeatureTest.Structure
{
    [TestFixture]
    [Target(@"(1, 2, 3, 4, 5, 6) dump_print")]
    [Output("(1, 2, 3, 4, 5, 6)")]
    [PrioTableTest]
    [ConversionService.Closure]
    public sealed class DumpPrint : CompilerTest {}
}