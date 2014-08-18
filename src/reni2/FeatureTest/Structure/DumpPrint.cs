using System.Linq;
using System.Collections.Generic;
using System;
using hw.UnitTest;
using Reni.Parser;

namespace Reni.FeatureTest.Structure
{
    [TestFixture]
    [Target(@"(1, 2, 3, 4, 5, 6) dump_print")]
    [Output("(1, 2, 3, 4, 5, 6)")]
    [PrioTableTest]
    public sealed class DumpPrint : CompilerTest
    {
        [Test]
        public override void Run() { BaseRun(); }
    }
}