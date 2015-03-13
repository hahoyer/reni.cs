using System;
using System.Collections.Generic;
using System.Linq;
using hw.UnitTest;
using Reni.FeatureTest.Helper;
using Reni.FeatureTest.TypeType;
using Reni.Parser;

namespace Reni.FeatureTest.Array
{
    [TestFixture]
    [Target("(<<5) type dump_print")]
    [Output("((number(bits:4))!!!3)*1")]
    [ParserTest]
    [TypeOperator]
    public sealed class TypeOfSimpleArrayFromPiece : CompilerTest
    {}
}