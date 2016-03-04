using System;
using System.Collections.Generic;
using System.Linq;
using hw.UnitTest;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.CleanupSection
{
    [UnitTest]
    [Basic]
    [TargetSet(@"This: {(a:1;b:3) a; 5 ~~~'end' dump_print} dump_print ", "(1, 5)end")]
    public sealed class Simple : CompilerTest {}
}             