using System;
using System.Collections.Generic;
using System.Linq;
using hw.UnitTest;
using Reni.FeatureTest.ConversionService;
using Reni.FeatureTest.Helper;
using Reni.ParserTest;

namespace Reni.FeatureTest.TypeType
{
    [UnitTest]
    [Closure, TypeOperator]
    public sealed class ApplyTypeOperator : CompilerTest
    {
        protected override string Target => @"(31 type instance (28))dump_print";
        protected override string Output => "28";
    }
}