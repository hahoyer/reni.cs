using System;
using System.Collections.Generic;
using System.Linq;
using hw.UnitTest;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.TypeType
{
    [UnitTest]
    [Target("31 type dump_print")]
    [Output("number(bits:6)")]
    public sealed class TypeOperator : CompilerTest {}
}