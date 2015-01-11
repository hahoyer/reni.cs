using System.Linq;
using System.Collections.Generic;
using System;
using hw.UnitTest;
using Reni.FeatureTest.ConversionService;

namespace Reni.FeatureTest.Structure
{
    [TestFixture]
    [Closure]
    [NumberExtensionConversion]
    [TargetSet(@"((0, 1) _A_T_ 0) dump_print", "0")]
    [TargetSet(@"((0, 1, ) _A_T_ 0) dump_print", "0")]
    public sealed class AccessSimple : CompilerTest {}

    [TestFixture]
    [TargetSet(@"1;1 dump_print", "1")]
    public sealed class TwoStatements : CompilerTest {}
}