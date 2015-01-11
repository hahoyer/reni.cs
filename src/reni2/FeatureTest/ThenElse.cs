using System;
using System.Collections.Generic;
using System.Linq;
using hw.UnitTest;
using Reni.FeatureTest.ConversionService;
using Reni.FeatureTest.Structure;

namespace Reni.FeatureTest.ThenElse
{
    [TestFixture]
    [Target(@"x: 1=1 then 1 else 100;x dump_print;")]
    [Output("1")]
    [InnerAccess]
    [SomeVariables]
    [Closure]
    public sealed class UseThen : CompilerTest {}

    [TestFixture]
    [Target(@"x: 1=0 then 1 else 100;x dump_print;")]
    [Output("100")]
    [InnerAccess]
    [SomeVariables]
    [Closure]
    public sealed class UseElse : CompilerTest {}
}