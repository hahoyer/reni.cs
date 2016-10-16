using System;
using System.Collections.Generic;
using System.Linq;
using hw.UnitTest;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest
{
    [UnitTest]
    [Target(@"!public x: 1")]
    [Output("")]
    public sealed class ScopeHandling : CompilerTest { }


    [UnitTest]
    [ScopeHandling]
    public sealed class AllScopeHandling : CompilerTest {}
}