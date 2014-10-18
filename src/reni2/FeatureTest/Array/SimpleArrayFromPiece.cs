using System.Linq;
using System.Collections.Generic;
using System;
using hw.UnitTest;
using Reni.Parser;

namespace Reni.FeatureTest.Array
{
    [TestFixture]
    [Target("(<<5) dump_print")]
    [Output("array(number(bits:4),(5))")]
    [ParserTest]
    public sealed class SimpleArrayFromPiece : CompilerTest
    {}
}