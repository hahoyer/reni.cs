using System;
using System.Collections.Generic;
using System.Linq;
using hw.UnitTest;
using Reni.FeatureTest.Helper;
using Reni.Parser;

namespace Reni.FeatureTest.TypeType
{
    [UnitTest]
    [PrioTableTest]
    [Target("31 type dump_print")]
    [Output("number(bits:6)")]
    public sealed class TypeOperator : CompilerTest {}
}