using System.Linq;
using System.Collections.Generic;
using System;
using hw.UnitTest;
using Reni.FeatureTest.Helper;

namespace Reni.FeatureTest.Function
{
    /// <summary>
    ///     Recursive function that will result in a stack overflow, except when compiled as a loop
    /// </summary>
    [UnitTest]
    [Target(@"!mutable i: 400000 type instance(400); f: /\i > 0 then (i := (i - 1)enable_cut; f());f()")]
    [Output("")]
    [PrimitiveRecursiveFunctionByteWithDump]
    public sealed class PrimitiveRecursiveFunctionSmall : CompilerTest
    {}
}