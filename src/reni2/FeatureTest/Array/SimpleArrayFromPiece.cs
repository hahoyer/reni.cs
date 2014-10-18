using System.Linq;
using System.Collections.Generic;
using System;
using hw.UnitTest;
using Reni.FeatureTest.TypeType;
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
    [Output("((number(bits:4))!!!3)*1")]
    [ParserTest]
    [TypeOperator]
    public sealed class TypeOfSimpleArrayFromPiece : CompilerTest
    {}
}