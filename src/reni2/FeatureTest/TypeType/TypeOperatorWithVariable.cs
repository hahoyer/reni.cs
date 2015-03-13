using System.Linq;
using System.Collections.Generic;
using System;
using hw.UnitTest;
using Reni.FeatureTest.Helper;
using Reni.FeatureTest.Structure;

namespace Reni.FeatureTest.TypeType
{
    [TestFixture]
    [InnerAccess]
    [SomeVariables]
    [TypeOperator]
    [TargetSet("x: 0; x type dump_print", "number(bits:1)")]
    public sealed class TypeOperatorWithVariable : CompilerTest
    {}
}