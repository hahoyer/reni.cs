using System.Linq;
using System.Collections.Generic;
using System;
using hw.UnitTest;
using Reni.Parser;

namespace Reni.FeatureTest.TypeType
{
    [TestFixture]
    [PrioTableTest]
    public sealed class ApplyTypeOperator : CompilerTest
    {
        protected override string Target { get { return @"(31 type instance (28))dump_print"; } }
        protected override string Output { get { return "28"; } }
        protected override IEnumerable<System.Type> DependsOn { get { return new[] {typeof(TypeOperator)}; } }

        [Test]
        public override void Run() { BaseRun(); }
    }
}