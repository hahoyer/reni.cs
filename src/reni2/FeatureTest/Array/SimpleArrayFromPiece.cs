using System.Linq;
using System.Collections.Generic;
using System;
using hw.UnitTest;
using Reni.Parser;

namespace Reni.FeatureTest.Array
{
    [TestFixture]
    [Target("(<<5) dump_print")]
    [Output("<<(5)")]
    [ParserTest]
    public sealed class SimpleArrayFromPiece : CompilerTest
    {}

    [TestFixture]
    [Target("(<<5) type dump_print")]
    [Output("Number(bits:4)*(1)")]
    [ParserTest]
    public sealed class TypeOfSimpleArrayFromPiece : CompilerTest
    {}
}