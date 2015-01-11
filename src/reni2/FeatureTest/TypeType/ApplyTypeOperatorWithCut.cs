using System.Linq;
using System.Collections.Generic;
using System;
using hw.UnitTest;
using Reni.FeatureTest.ConversionService;

namespace Reni.FeatureTest.TypeType
{
    [TestFixture]
    [ApplyTypeOperator]
    [NumberPointerCutConversion]
    public sealed class ApplyTypeOperatorWithCut : CompilerTest
    {
        protected override string Target => @"31 type instance (100 enable_cut) dump_print";
        protected override string Output => "-28";
    }
}