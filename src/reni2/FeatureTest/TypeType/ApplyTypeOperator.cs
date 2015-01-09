using System.Linq;
using System.Collections.Generic;
using System;
using hw.UnitTest;
using Reni.Parser;

namespace Reni.FeatureTest.TypeType
{
    [TestFixture]
    [PrioTableTest]
    [ConversionService.Closure]
    public sealed class ApplyTypeOperator : CompilerTest
    {
        protected override string Target => @"(31 type instance (28))dump_print";
        protected override string Output => "28";
        protected override IEnumerable<System.Type> DependsOn => new[] {typeof(TypeOperator)};
    }
}