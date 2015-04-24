using System.Linq;
using System.Collections.Generic;
using System;
using hw.UnitTest;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.Array
{
    [UnitTest]
    [Target("(<<5) dump_print")]
    [Output("<<(5)")]
    [ParserTest.ParserTest]
    public sealed class SimpleArrayFromPiece : CompilerTest
    {}
}