using System.Linq;
using System.Collections.Generic;
using System;
using hw.UnitTest;

namespace Reni.FeatureTest.TypeType
{
    [TestFixture]
    [ApplyTypeOperator]
    public sealed class ApplyTypeOperatorWithCut : CompilerTest
    {
        protected override string Target { get { return @"31 type instance (100 enable_cut) dump_print"; } }
        protected override string Output { get { return "-28"; } }
        protected override IEnumerable<System.Type> DependsOn { get { return new[] {typeof(ApplyTypeOperator)}; } }
    }
}