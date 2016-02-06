using System;
using System.Collections.Generic;
using System.Linq;
using hw.Helper;
using hw.UnitTest;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest
{
    [UnitTest]
    [Output("Hallo")]
    public sealed class ComplexContext : CompilerTest
    {
        protected override string Target => "FeatureTest\\ComplexContext.reni".FileHandle().String;
    }
}