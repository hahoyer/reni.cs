using System;
using System.Collections.Generic;
using System.Linq;
using hw.UnitTest;

namespace Reni.FeatureTest.Array
{
    [TestFixture]
    [Target(@"(5 type * 5) instance (/\ ^) dump_print")]
    [Output("<<(0, 1, 2, 3, 4)")]
    [DefaultInitialized]
    public sealed class FromTypeAndFunction : CompilerTest
    {}
}