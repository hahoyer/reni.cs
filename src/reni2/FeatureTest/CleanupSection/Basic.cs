using System;
using System.Collections.Generic;
using System.Linq;
using hw.UnitTest;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.CleanupSection
{
    [UnitTest]
    [TargetSet(@"This: {~~~}; This dump_print ", "()")]
    [TargetSet(@"This: {3~~~}; This dump_print ", "(3)")]
    [TargetSet(@"This: {~~~()}; This dump_print ", "()")]
    [TargetSet(@"This: {3~~~()}; This dump_print ", "(3)")]
    public sealed class Basic : CompilerTest {}
}