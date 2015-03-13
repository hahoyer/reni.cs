using System.Linq;
using System.Collections.Generic;
using System;
using hw.UnitTest;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.Array
{
    [TestFixture]
    [Target("(5 type * 5) instance () dump_print")]
    [Output("<<(0, 0, 0, 0, 0)")]
    public sealed class DefaultInitialized : CompilerTest
    {}
}