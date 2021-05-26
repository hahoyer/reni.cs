using hw.UnitTest;
using Reni.FeatureTest.Array;
using Reni.FeatureTest.ConversionService;
using Reni.FeatureTest.Helper;
using Reni.FeatureTest.Structure;

namespace Reni.FeatureTest.ThenElse
{
    [UnitTest]
    [Target(@"x: 1=1 then 1 else 100;x dump_print;")]
    [Output("1")]
    [InnerAccess]
    [SomeVariables]
    [Closure]
    public sealed class UseThen : CompilerTest
    {}

    [UnitTest]
    [Target(@"x: 1=0 then 1 else 100;x dump_print;")]
    [Output("100")]
    [InnerAccess]
    [SomeVariables]
    [Closure]
    public sealed class UseElse : CompilerTest
    {}

    [UnitTest]
    [Target(@"x: <<5<<3; y: 1=0 then x(0) else x(1);y dump_print;")]
    [Output("3")]
    [UseElse, ArrayVariable]
    public sealed class AutomaticDereferencing : CompilerTest
    {}
}