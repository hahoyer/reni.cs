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
    public sealed class ScopeHandlingPublic : CompilerTest {}

    [UnitTest]
    [Target(@"!(public mutable) x: 1")]
    [ScopeHandlingPublic]
    [Output("")]
    public sealed class ScopeHandlingGroup : CompilerTest {}

    [UnitTest]
    [Target(@"!public !mutable x: 1")]
    [ScopeHandlingPublic]
    [Output("")]
    public sealed class ScopeHandlingMultiple : CompilerTest {}

    [UnitTest]
    [Target(@"!unkown x: 1")]
    [ScopeHandlingPublic]
    [Output("")]
    public sealed class ScopeHandlingError : CompilerTest {}

    [UnitTest]
    [ScopeHandlingPublic]
    [ScopeHandlingGroup]
    [ScopeHandlingError]
    [ScopeHandlingMultiple]
    public sealed class AllScopeHandling : CompilerTest {}
}