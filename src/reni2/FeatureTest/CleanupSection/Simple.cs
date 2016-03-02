using System;
using System.Collections.Generic;
using System.Linq;
using hw.UnitTest;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.CleanupSection
{
    [UnitTest][Basic]
    [TargetSet(@"This: {~~~'end' dump_print}; This dump_print ", "()end")]
    public sealed class Simple: CompilerTest { }
}